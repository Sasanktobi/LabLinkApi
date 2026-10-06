using System;
using System.Globalization;
using Backend.Constants;
using Backend.DTOs;
using Backend.Helpers;
using Backend.IServices;

namespace Backend.Services
{
    public class ReportPdfGenerator : IReportPdfGenerator
    {
        private const float Left=40f;
        private const float Right=SimplePdfDocument.PageWidth - 40f;
        private const float BottomLimit=780f;

        // Column x-positions for the results table.
        private const float ColTest=Left + 4;
        private const float ColResult=250f;
        private const float ColUnit=330f;
        private const float ColRange=400f;
        private const float ColFlag=510f;

        public byte[] Generate(ReportResponseDto report)
        {
            var pdf=new SimplePdfDocument();
            var y=DrawHeader(pdf, report);

            y=DrawPatientBlock(pdf, report, y);
            y=DrawTableHeader(pdf, y);

            foreach (var result in report.Results)
            {
                var remarkLines=string.IsNullOrWhiteSpace(result.Remarks)
                    ? new System.Collections.Generic.List<string>()
                    : SimplePdfDocument.Wrap(result.Remarks, ColFlag - ColTest - 10, 8);

                var rowHeight=16f + remarkLines.Count * 10f;
                if (y + rowHeight > BottomLimit)
                {
                    y=StartContinuationPage(pdf, report);
                    y=DrawTableHeader(pdf, y);
                }

                var abnormal=result.Flag == ResultFlags.Low || result.Flag == ResultFlags.High;
                pdf.Text(ColTest, y, SimplePdfDocument.Truncate(result.TestName, ColResult - ColTest - 6, 10), 10);
                pdf.Text(ColResult, y, SimplePdfDocument.Truncate(result.ResultValue, ColUnit - ColResult - 6, 10), 10, abnormal);
                pdf.Text(ColUnit, y, SimplePdfDocument.Truncate(result.Unit, ColRange - ColUnit - 6, 10), 10);
                pdf.Text(ColRange, y, SimplePdfDocument.Truncate(FormatRange(result), ColFlag - ColRange - 6, 10), 10);
                pdf.Text(ColFlag, y, result.Flag, 10, abnormal);

                var remarkY=y + 11;
                foreach (var line in remarkLines)
                {
                    pdf.Text(ColTest + 8, remarkY, line, 8);
                    remarkY+=10;
                }

                y+=rowHeight;
                pdf.Line(Left, y - 11, Right, y - 11, 0.25f);
            }

            y+=10;
            var remarks=string.IsNullOrWhiteSpace(report.PathologistRemarks) ? "None." : report.PathologistRemarks;
            var lines=SimplePdfDocument.Wrap(remarks, Right - Left, 10);
            if (y + 60 + lines.Count * 13 > BottomLimit)
            {
                y=StartContinuationPage(pdf, report);
            }

            pdf.Text(Left, y, "Pathologist's Remarks", 11, true);
            y+=15;
            foreach (var line in lines)
            {
                pdf.Text(Left, y, line, 10);
                y+=13;
            }

            y+=20;
            pdf.Text(Left, y, $"Validated by: {report.ValidatedByName ?? "-"}", 10, true);
            y+=13;
            pdf.Text(Left, y, $"Validated on: {FormatDateTime(report.ValidatedAt)}", 10);
            y+=13;
            pdf.Text(Left, y, $"Released on: {FormatDateTime(report.ReleasedAt)}", 10);

            y+=25;
            pdf.Text(Left, y, "Results flagged High/Low are outside the reference range. Please consult your doctor for interpretation.", 8);

            return pdf.Build();
        }

        private static float DrawHeader(SimplePdfDocument pdf, ReportResponseDto report)
        {
            pdf.Text(Left, 50, "LabLink Diagnostics", 20, true);
            pdf.Text(Left, 68, "Laboratory Test Report", 12);
            pdf.Text(Right - 120, 50, $"Report #{report.ReportId}", 10, true);
            pdf.Text(Right - 120, 64, $"Appointment #{report.AppointmentId}", 9);
            pdf.Line(Left, 80, Right, 80, 1f);
            pdf.Text(Left, SimplePdfDocument.PageHeight - 25, $"LabLink Diagnostics - Report #{report.ReportId} - Page {pdf.PageCount}", 8);
            return 100f;
        }

        private static float StartContinuationPage(SimplePdfDocument pdf, ReportResponseDto report)
        {
            pdf.NewPage();
            var y=DrawHeader(pdf, report);
            pdf.Text(Left, y, $"{report.PatientName} (continued)", 10, true);
            return y + 20;
        }

        private static float DrawPatientBlock(SimplePdfDocument pdf, ReportResponseDto report, float y)
        {
            var mid=Left + (Right - Left) / 2;

            pdf.Text(Left, y, "Patient:", 10, true);
            pdf.Text(Left + 75, y, report.PatientName, 10);
            pdf.Text(mid, y, "Patient ID:", 10, true);
            pdf.Text(mid + 85, y, report.PatientId.ToString(CultureInfo.InvariantCulture), 10);
            y+=15;

            pdf.Text(Left, y, "Age / Sex:", 10, true);
            pdf.Text(Left + 75, y, $"{report.PatientAge} yrs / {report.PatientGender}", 10);
            pdf.Text(mid, y, "Sample date:", 10, true);
            pdf.Text(mid + 85, y, report.ScheduleDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture), 10);
            y+=15;

            pdf.Text(Left, y, "Referred by:", 10, true);
            pdf.Text(Left + 75, y, string.IsNullOrWhiteSpace(report.DoctorName) ? "Self" : $"Dr. {report.DoctorName}", 10);
            pdf.Text(mid, y, "Report date:", 10, true);
            pdf.Text(mid + 85, y, FormatDateTime(report.ReleasedAt ?? report.ValidatedAt), 10);
            y+=25;

            return y;
        }

        private static float DrawTableHeader(SimplePdfDocument pdf, float y)
        {
            pdf.FillRect(Left, y - 12, Right - Left, 18, 0.88f);
            pdf.Text(ColTest, y, "Test", 10, true);
            pdf.Text(ColResult, y, "Result", 10, true);
            pdf.Text(ColUnit, y, "Unit", 10, true);
            pdf.Text(ColRange, y, "Reference Range", 10, true);
            pdf.Text(ColFlag, y, "Flag", 10, true);
            return y + 22;
        }

        private static string FormatRange(TestResultResponseDto result)
        {
            var low=result.ReferenceRangeLow;
            var high=result.ReferenceRangeHigh;

            if (!string.IsNullOrWhiteSpace(low) && !string.IsNullOrWhiteSpace(high))
            {
                return $"{low} - {high}";
            }
            if (!string.IsNullOrWhiteSpace(low))
            {
                return $">= {low}";
            }
            if (!string.IsNullOrWhiteSpace(high))
            {
                return $"<= {high}";
            }
            return "-";
        }

        private static string FormatDateTime(DateTime? utc)
        {
            return utc.HasValue
                ? utc.Value.ToString("dd MMM yyyy HH:mm 'UTC'", CultureInfo.InvariantCulture)
                : "-";
        }
    }
}
