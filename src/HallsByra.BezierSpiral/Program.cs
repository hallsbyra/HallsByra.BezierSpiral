using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using static HallsByra.BezierSpiral.BezierSpiral;

namespace HallsByra.BezierSpiral
{
    class Program
    {
        static void Main(string[] args)
        {
            var curves = BezierSpiral.Generate(
                center: new Point(0, 0),
                startAngle: 0,
                endAngle:  Math.PI * 2 * 5,
                angleStep: Math.PI / 2,
                turnSeparation: 1);

            var curveSvgs = String.Join(Environment.NewLine,
                curves.Select(c => Svg.Bezier(c)));

            var lineSvgs = String.Join(Environment.NewLine,
                curves.SelectMany(c => new[] { Svg.Line(c.P0, c.P1, "blue"), Svg.Line(c.P3, c.P2) }));

            var pointSvgs = String.Join(Environment.NewLine,
                curves.SelectMany(c => new[] { Svg.Circle(c.P0), Svg.Circle(c.P1), Svg.Circle(c.P2), Svg.Circle(c.P3) }));

            var html = $@"
<body>
    <svg width=""500"" height=""500"" viewBox=""-6 -6 12 12"" xmlns=""http://www.w3.org/2000/svg"">
        {curveSvgs}
        {lineSvgs}
        {pointSvgs}
    </svg>
</body>";
            ShowInBrowser(html);
        }

        static void ShowInBrowser(string html)
        {
            var filePath = Path.GetFullPath("index.html");
            File.WriteAllText(filePath, html);
            OpenBrowser($"file://{filePath}");
        }

        static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Process.Start("xdg-open", url);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Process.Start("open", url);
            else
                throw new NotSupportedException();
        }
    }

    static class Svg
    {
        public static string Line(Point p1, Point p2, string stroke = "green") =>
            FormattableString.Invariant($@"<line x1=""{p1.X:0.000}"" y1=""{p1.Y:0.000}"" x2=""{p2.X:0.000}"" y2=""{p2.Y:0.000}"" stroke=""{stroke}"" stroke-width=""0.025""/>");
        public static string Circle(Point p) =>
            FormattableString.Invariant($@"<circle cx=""{p.X:0.000}"" cy=""{p.Y:0.000}"" r=""0.05"" stroke=""none"" fill=""red""/>");
        public static string Bezier(Bezier c) =>
            FormattableString.Invariant($@"<path d=""M{c.P0.X:0.000} {c.P0.Y:0.000} C {c.P1.X:0.000} {c.P1.Y:0.000}, {c.P2.X:0.000} {c.P2.Y:0.000}, {c.P3.X:0.000} {c.P3.Y:0.000}"" stroke=""black"" stroke-width=""0.1"" fill=""none""/>");
    }
}
