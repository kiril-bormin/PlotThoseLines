<img style="display: block; margin: 0 auto;" src="docs/logo.png" alt="Logo" width="500" />

1. C#, un outil simple et léger pour tracer et visualiser des lignes et courbes de données, MainForm - la logicique et le fichier principal, mise en place des éléments dans la fenêtre. SavedSerie - Classe pour séparer les points en data points. Dossier service contient les méthodes, tels que : transformer un nombre en format plus court, Parser des fichier json, et une méthode qui fussione des serie s'il ont des dates communes

2. Roslynator https://github.com/dotnet/roslynator et Reshaper Command Line Tools https://www.jetbrains.com/help/resharper/ReSharper_Command_Line_Tools.html

3. C'est un outil opensource et bien noté par les développeurs en C#. Il scanne le code pour détecter des bugs, du code mort, des risques de mauvaise conception, etc.

4. git checkout -b linter-test

5. ```dotnet tool install -g roslynator.dotnet.cli``` et ```roslynator --version```

6. ```roslynator analyze .\PlotThoseLines.sln``` **56 diagnostics found**