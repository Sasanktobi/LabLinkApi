using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.DTOs;
using Backend.Exceptions;
using Backend.Helpers;
using Backend.IRepositories;
using Backend.IServices;
using Backend.Models;

namespace Backend.Services
{
    public class TestCatalogueService : ITestCatalogueService
    {
        private readonly ITestRepository testRepository;
        private readonly ITestPanelRepository panelRepository;
        private readonly IAuditService auditService;

        public TestCatalogueService(ITestRepository _testRepository, ITestPanelRepository _panelRepository, IAuditService _auditService)
        {
            testRepository=_testRepository;
            panelRepository=_panelRepository;
            auditService=_auditService;
        }

        public async Task<IEnumerable<TestResponseDto>> GetTestsAsync(string? search, bool includeInactive)
        {
            var tests=await testRepository.GetAllAsync(search, includeInactive);
            return tests.Select(DtoMapper.ToDto).ToList();
        }

        public async Task<TestResponseDto> GetTestAsync(int id)
        {
            return DtoMapper.ToDto(await LoadTestAsync(id));
        }

        public async Task<TestResponseDto> CreateTestAsync(TestUpsertDto dto)
        {
            await EnsureTestUniqueAsync(dto, null);

            var test=new Test();
            Apply(test, dto);
            await testRepository.AddAsync(test);

            await auditService.LogAsync(nameof(Test), test.TestId, AuditActions.Create, $"Created test {test.Code} - {test.Name}.");
            return DtoMapper.ToDto(test);
        }

        public async Task<TestResponseDto> UpdateTestAsync(int id, TestUpsertDto dto)
        {
            var test=await LoadTestAsync(id);
            await EnsureTestUniqueAsync(dto, id);

            // Already-ordered tests keep their PriceAtOrder, so price changes only affect new bookings.
            Apply(test, dto);
            await testRepository.SaveChangesAsync();

            await auditService.LogAsync(nameof(Test), id, AuditActions.Update, $"Updated test {test.Code}.");
            return DtoMapper.ToDto(test);
        }

        public async Task SetTestActiveAsync(int id, bool isActive)
        {
            var test=await LoadTestAsync(id);
            test.IsActive=isActive;
            await testRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(Test), id, AuditActions.StatusChange, isActive ? "Activated test." : "Deactivated test.");
        }

        public async Task<IEnumerable<TestPanelResponseDto>> GetPanelsAsync(bool includeInactive)
        {
            var panels=await panelRepository.GetAllAsync(includeInactive);
            return panels.Select(DtoMapper.ToDto).ToList();
        }

        public async Task<TestPanelResponseDto> GetPanelAsync(int id)
        {
            return DtoMapper.ToDto(await LoadPanelAsync(id));
        }

        public async Task<TestPanelResponseDto> CreatePanelAsync(TestPanelUpsertDto dto)
        {
            var name=dto.Name.Trim();
            if (await panelRepository.NameExistsAsync(name, null))
            {
                throw new ConflictException($"A panel named '{name}' already exists.");
            }

            var tests=await LoadActiveTestsAsync(dto.TestIds);
            var panel=new TestPanel
            {
                Name=name,
                Description=dto.Description,
                Price=dto.Price,
                TestPanelTests=tests.Select(t=>new TestPanelTest { TestId=t.TestId, Test=t }).ToList()
            };

            await panelRepository.AddAsync(panel);
            await auditService.LogAsync(nameof(TestPanel), panel.TestPanelId, AuditActions.Create, $"Created panel {panel.Name} with {tests.Count} test(s).");
            return DtoMapper.ToDto(panel);
        }

        public async Task<TestPanelResponseDto> UpdatePanelAsync(int id, TestPanelUpsertDto dto)
        {
            var panel=await LoadPanelAsync(id);
            var name=dto.Name.Trim();
            if (await panelRepository.NameExistsAsync(name, id))
            {
                throw new ConflictException($"A panel named '{name}' already exists.");
            }

            var tests=await LoadActiveTestsAsync(dto.TestIds);
            var wanted=tests.Select(t=>t.TestId).ToHashSet();

            panel.Name=name;
            panel.Description=dto.Description;
            panel.Price=dto.Price;

            foreach (var link in panel.TestPanelTests.Where(pt=>!wanted.Contains(pt.TestId)).ToList())
            {
                panel.TestPanelTests.Remove(link);
            }

            var current=panel.TestPanelTests.Select(pt=>pt.TestId).ToHashSet();
            foreach (var test in tests.Where(t=>!current.Contains(t.TestId)))
            {
                panel.TestPanelTests.Add(new TestPanelTest { TestPanelId=id, TestId=test.TestId, Test=test });
            }

            await panelRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(TestPanel), id, AuditActions.Update, $"Updated panel {panel.Name}.");
            return DtoMapper.ToDto(panel);
        }

        public async Task SetPanelActiveAsync(int id, bool isActive)
        {
            var panel=await LoadPanelAsync(id);
            panel.IsActive=isActive;
            await panelRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(TestPanel), id, AuditActions.StatusChange, isActive ? "Activated panel." : "Deactivated panel.");
        }

        private async Task<Test> LoadTestAsync(int id)
        {
            return await testRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Test {id} was not found.");
        }

        private async Task<TestPanel> LoadPanelAsync(int id)
        {
            return await panelRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Test panel {id} was not found.");
        }

        private async Task<List<Test>> LoadActiveTestsAsync(List<int> testIds)
        {
            var ids=testIds.Distinct().ToList();
            var tests=await testRepository.GetByIdsAsync(ids);

            var missing=ids.Except(tests.Select(t=>t.TestId)).ToList();
            if (missing.Count > 0)
            {
                throw new BadRequestException($"Unknown test id(s): {string.Join(", ", missing)}.");
            }

            var inactive=tests.Where(t=>!t.IsActive).Select(t=>t.Code).ToList();
            if (inactive.Count > 0)
            {
                throw new BadRequestException($"Inactive test(s) cannot be added to a panel: {string.Join(", ", inactive)}.");
            }

            return tests;
        }

        private async Task EnsureTestUniqueAsync(TestUpsertDto dto, int? excludeId)
        {
            if (await testRepository.NameExistsAsync(dto.Name.Trim(), excludeId))
            {
                throw new ConflictException($"A test named '{dto.Name.Trim()}' already exists.");
            }

            if (await testRepository.CodeExistsAsync(dto.Code.Trim().ToUpperInvariant(), excludeId))
            {
                throw new ConflictException($"A test with code '{dto.Code.Trim().ToUpperInvariant()}' already exists.");
            }
        }

        private static void Apply(Test test, TestUpsertDto dto)
        {
            test.Name=dto.Name.Trim();
            test.Code=dto.Code.Trim().ToUpperInvariant();
            test.Description=dto.Description;
            test.SampleType=dto.SampleType.Trim();
            test.Price=dto.Price;
            test.ReferenceRangeLow=dto.ReferenceRangeLow.Trim();
            test.ReferenceRangeHigh=dto.ReferenceRangeHigh.Trim();
            test.Unit=dto.Unit.Trim();
            test.TurnaroundHours=dto.TurnaroundHours;
        }
    }
}
