# LTC2.PC4
Long Term NL Postcode Challenge

Zorg voor je begint voor het volgende:

- een computer met Windows 11 met minimaal 16 GByte intern geheugen
- de command line git client moeten zijn geinstalleerd op deze computer
- de command line build tools van dotnet 10 moeten zijn geinstalleerd op deze computer
- node.js moet geinstalleerd zijn op deze computer

Doorloop de volgende stappen voor het bouwen en starten van de challenge applicatie:

- Stap 1: download deze file: https://whitemill20studios.nl/longtermnlpostcodechallenge/main/LTC2.PC4.zip
- Stap 2: unzip deze file naar een folder op je file system
- Stap 3: open een command line prompt (cmd.exe) en ga naar de folder waar je de zip file hebt uitgepakt
- Stap 4: hier vind je een subfolder "scripts" en ga vanuit je command line prompt naar deze "scripts" folder
- Stap 5: start in deze folder eerst het batch bestand: "cloneFromGitHub.cmd"
- Stap 6: vervolgens start je vanuit dezelfde folder: "buildAllDebug.cmd"
- Stap 7: is het bouwen gelukt dan kun je de applicatie starten, lees eerst de "ReadMe.pdf" uit de documents folder
- Stap 8: het starten van de gebouwde applicatie kan met het script: "runApplication.cmd"

Optioneel:

- Stap 9: het Archive Importer Tool is te starten met: "runArchiveImporter.cmd"

'Werk in uitvoering':

- Dit project is opgezet als een tool dat oorspronkelijk alleen op Windows kon worden gebruikt
- Er heeft ooit een Mac/OSX versie bestaan maar die is nooit 'ge-open-sourced' vanwege licentieredenen
- Inmiddels is er een 'back-port' van die versie naar deze repository gedaan
- De oorspronkelijke issues met licentie zitten niet meer in deze back-port
- Het is echter nog steeds werk in uitvoering en nog niet volledig geschikt voor niet-Windows platform
- Desalniettemin, het begin is er en enthousiastelingen zouden het kunnen oppakken
- Het 'buildAllDebug.cmd' script bouwt ook deze 'generieke' applicatie
- Deze applicatie is na het bouwen te starten met 'runGenericApplication.cmd'
- Het is nog steeds iets wat in ontwikkeling is dus mogelijk niet 100% stabiel
- Als je eerder de zip file met script downloadde, dan moet je deze opnieuw downloaden en uitpakken
