using System.Text.Json;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== OsiedlaMapGenerator SVG ===\n");

        string dataDir = "../data/";
        string outputRoot = "../dist/";
        Directory.CreateDirectory(outputRoot);

        var jsonFiles = Directory.GetFiles(dataDir, "*.json");

        if (jsonFiles.Length == 0)
        {
            Console.WriteLine("Brak plików .json w folderze /data/");
            Console.WriteLine("Wrzuć GeoJSON z granicami i uruchom ponownie.");
            return;
        }

        Console.WriteLine($"Znaleziono plików danych: {jsonFiles.Length}\n");

        var indexLinks = new List<string>();

        foreach (var jsonFile in jsonFiles)
        {
            string fileName = Path.GetFileName(jsonFile);
            string projectName = CleanName(Path.GetFileNameWithoutExtension(fileName));
            string outputFolder = Path.Combine(outputRoot, projectName);

            Directory.CreateDirectory(outputFolder);

            Console.WriteLine($"=> Przetwarzam: {fileName}");
            Console.WriteLine($"   Obszar: {projectName}");

            int featureCount = GenerateMap(jsonFile, outputFolder, projectName);

            if (featureCount > 0)
                indexLinks.Add($@"
<div class='card'>
    <div class='card-title'>{projectName}</div>
    <small>{featureCount} jednostek</small>
    <a href='./{projectName}/index.html'>Otwórz mapę</a>
</div>");

            Console.WriteLine($"✔ Wyniki: dist/{projectName}/\n");
        }

// Globalny index.html
string mainIndex = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset=""UTF-8"">
<title>Mapa dostępnych obszarów</title>
<style>
body {{
    font-family: Arial, sans-serif;
    margin: 0;
    background:#f4f4f4;
}}
header {{
    background: #222;
    color: white;
    padding: 20px 30px;
    font-size: 22px;
}}
main {{
    padding: 30px;
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
    gap: 20px;
}}
.card {{
    background: white;
    border-radius: 8px;
    padding: 20px;
    box-shadow: 0 2px 6px rgba(0,0,0,0.15);
    transition: transform .2s, box-shadow .2s;
}}
.card:hover {{
    transform: translateY(-4px);
    box-shadow: 0 4px 10px rgba(0,0,0,0.25);
}}
.card-title {{
    font-size: 18px;
    margin-bottom: 8px;
    font-weight: bold;
}}
.card small {{
    display: block;
    margin-bottom: 10px;
    color: #555;
}}
.card a {{
    display: inline-block;
    background: #005eff;
    color: white;
    padding: 6px 12px;
    text-decoration: none;
    border-radius: 5px;
}}
.card a:hover {{
    background: #003ec1;
}}
</style>
</head>
<body>
<header> 📍 Dostępne mapy </header>
<main>
{string.Join("\n", indexLinks)}
</main>
</body>
</html>";

        File.WriteAllText(Path.Combine(outputRoot, "index.html"), mainIndex);

        Console.WriteLine("=== GOTOWE ===");
        Console.WriteLine("Otwórz dist/index.html jako stronę startową\n");
    }

    static int GenerateMap(string input, string outputDir, string title)
    {
        var geoJson = File.ReadAllText(input);
        var json = JsonDocument.Parse(geoJson);

        var features = json.RootElement.GetProperty("features").EnumerateArray().ToList();
        Console.WriteLine($"   • Jednostek: {features.Count}");

        if (features.Count == 0) return 0;

        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        foreach (var feat in features)
        {
            var coords = feat.GetProperty("geometry").GetProperty("coordinates");
            foreach (var poly in coords.EnumerateArray())
                foreach (var ring in poly.EnumerateArray())
                    foreach (var pt in ring.EnumerateArray())
                    {
                        double x = pt[0].GetDouble();
                        double y = pt[1].GetDouble();
                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y);
                        maxY = Math.Max(maxY, y);
                    }
        }

        StringBuilder paths = new();

        foreach (var feat in features)
        {
            string name = feat.GetProperty("properties").GetProperty("name").GetString()!;
            var coords = feat.GetProperty("geometry").GetProperty("coordinates");
            StringBuilder d = new();

            foreach (var poly in coords.EnumerateArray())
                foreach (var ring in poly.EnumerateArray())
                {
                    bool first = true;
                    foreach (var pt in ring.EnumerateArray())
                    {
                        double x = pt[0].GetDouble();
                        double y = pt[1].GetDouble();
                        d.Append(first ? $"M {x} {-y} " : $"L {x} {-y} ");
                        first = false;
                    }
                    d.Append("Z ");
                }

            paths.Append($@"
<path class=""region"" d=""{d}"" data-name=""{name}"">
    <title>{name}</title>
</path>");
        }

        string html = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset=""UTF-8"">
<title>{title} — Mapa jednostek</title>
<style>
body {{ margin:0; overflow:hidden; font-family:Arial,sans-serif; }}
svg  {{ width:100vw; height:100vh; cursor:grab; background:#fafafa; }}
.region {{ fill:#ccc; stroke:#000; stroke-width:0.00025; transition:fill .12s; }}
.region:hover {{ fill:#999; }}
#info {{
    position:fixed; top:10px; left:10px;
    background:#000; color:#fff;
    padding:7px 11px; border-radius:6px; opacity:.85;
}}
</style>
</head>
<body>

<div id=""info"">Kliknij jednostkę…</div>

<svg id=""map"" xmlns=""http://www.w3.org/2000/svg"" 
     viewBox=""{minX} {-maxY} {maxX-minX} {maxY-minY}"">
{paths}
</svg>

<script>
let svg = document.getElementById('map');
let isPan=false, sx, sy;
let vb = svg.viewBox.baseVal;

// PAN
svg.addEventListener('mousedown', e => {{
    isPan=true; svg.style.cursor='grabbing';
    sx=e.clientX; sy=e.clientY;
}});
svg.addEventListener('mouseup', () => {{
    isPan=false; svg.style.cursor='grab';
}});
svg.addEventListener('mousemove', e => {{
    if(!isPan) return;
    vb.x -= (e.clientX - sx)*(vb.width/window.innerWidth);
    vb.y += (e.clientY - sy)*(vb.height/window.innerHeight);
    sx=e.clientX; sy=e.clientY;
}});

// ZOOM
svg.addEventListener('wheel', e => {{
    e.preventDefault();
    const scale = e.deltaY>0 ? 1.1 : 0.9;
    vb.width*=scale;
    vb.height*=scale;
}});

// CLICK SELECT
document.querySelectorAll('.region').forEach(r => {{
    r.onclick = e => {{
        document.querySelectorAll('.region').forEach(a => a.style.fill='#ccc');
        e.target.style.fill='#666';
        document.getElementById('info').innerText = e.target.dataset.name;
    }};
}});
</script>
</body>
</html>";

        File.WriteAllText(Path.Combine(outputDir, "index.html"), html);
        return features.Count;
    }

    static string CleanName(string name)
    {
        string n = name.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in n)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        n = sb.ToString();
        n = Regex.Replace(n, @"[^A-Za-z0-9]+", "_");
        return n.Trim('_');
    }
}