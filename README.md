# OsiedlaMapGenerator

Generator interaktywnych map osiedli/dzielnic w formacie **SVG + HTML**, oparty o dane **GeoJSON**.  
Obsługuje **dowolną liczbę plików** w katalogu `data/` — każdy plik tworzy osobną mapę.

Wynik jest gotowy do otwarcia w przeglądarce, bez serwera.


## Funkcje

- automatyczne przetwarzanie wielu plików GeoJSON naraz
- generowanie folderów w `dist/` zawierających:
  - interaktywną mapę (`index.html`)
  - osiedla jako elementy SVG z nazwą
- globalny `dist/index.html` jako strona wyboru mapy
- zoom myszką oraz przesuwanie mapy (pan)
- kliknięte osiedle wyróżnia się i pokazuje nazwę


## Wymagania

- .NET 8.0 lub nowszy
- GeoJSON z geometrią typu `Polygon` lub `MultiPolygon`
- Pole `properties.name` musi zawierać nazwę jednostki


## Struktura projektu

```
/data/      ← tutaj wrzucasz GeoJSON
/dist/      ← tutaj pojawiają się wygenerowane mapy
/src/       ← kod źródłowy (Program.cs)
README.md
```

Po uruchomieniu:

```
dist/
 ├── Poznan/
 │    └── index.html
 ├── Warszawa/
 │    └── index.html
 └── index.html   ← lista dostępnych map
```


## Jak używać

1. Umieść pliki `.json` z GeoJSON w katalogu:

```
data/
  Poznan.json
  Warszawa.json
  DowolneMiasto.json
```

2. Uruchom generator:

```
dotnet run
```

3. Otwórz wygenerowaną stronę:

- `dist/index.html` → lista map
- kliknij wybrane miasto → interaktywna mapa


## Przykładowy log z konsoli

```
Znaleziono plików danych: 3
=> Przetwarzam: Poznan.json
   • Jednostek: 42
✔ Wyniki: dist/Poznan/

=> Przetwarzam: Warszawa.json
   • Jednostek: 18
✔ Wyniki: dist/Warszawa/

=== GOTOWE ===
```


## Format danych GeoJSON

Minimalny przykład:

```json
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "properties": { "name": "Wilda" },
      "geometry": {
        "type": "Polygon",
        "coordinates": [
          [[16.90,52.39],[16.92,52.39],[16.92,52.38],[16.90,52.38],[16.90,52.39]]
        ]
      }
    }
  ]
}
```


## Możliwości rozwoju

- animowane centrowanie na kliknięte osiedle (zoom-to-feature)
- panel informacji o osiedlu (dodatkowe właściwości GeoJSON)
- wyszukiwarka osiedli
- eksport całej mapy do jednego pliku SVG lub PNG
- opcjonalny hosting GitHub Pages


## Licencja

MIT License

Copyright (c) 2025 michalkuzdowicz

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.