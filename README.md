# Sanitizzatore LPG Neo4J

## Indice 
- [Descrizione](#Descrizione)
- [Librerie e framework impiegati](#Librerie-e-framework-impiegati)
- [Requisiti e setup](#Requisiti-e-setup)
## Descrizione

Questo progetto si pone lo scopo di creare un software che, fornite informazioni su cosa sia sensibile all'interno di un LPG Neo4J, permetta di sanitizzare la base di dati andando a creare autonomamente le query da inviare al DBMS, velocizzando le operazioni richieste ad un amministratore di rete che avesse la necessità di condividere una base di dati anonimizzata.
L'applicativo richiede all'amministratore di creare un file JSON contenete le specifiche che permetteranno di creare le query di sanitizzazione tramite la definizioni di quali nodi e/o relazioni rappresentino in toto o in parte un dato sensibile.

## Librerie e framework impiegati

Il software è stato sviluppato interamente in C\# e fa uso delle seguenti tecnologie:
- libreria [Neo4jClient](https://github.com/DotNet4Neo4j/Neo4jClient): è stata utilizzata per gestire la comunicazione col database Neo4J, nonché per la grenerazione delle query impiegate per la sanitizzazione;
- framework [Avalonia UI](https://avaloniaui.net/): lo sviluppo dell'intero applicativo è stato condotto secondo pattern architetturale MVVM per permette l'uso di questo framework nella costruzione delle interfacce grafice, oltre che per conferire maggiore manutenibilità al codice;
- framework [Newtonsoft](https://www.newtonsoft.com/json): questo framework è stato utilizzato per effettuare il parsing del file JSON ricevuto come input.


## Requisiti e setup

L'unico requisito richiesto dal progetto è quello di avere installato correttamente la piattaforma dotnet al fine di poter compilare il sorgente in maniera adatta al proprio sistema operativo.
Per maggiori informazioni condurre il processo di installazione, consultare la [guida fornita da Microsoft](https://dotnet.microsoft.com/en-us/download).
Di seguito due esempi per la compilazione del sorgente:
```C
// compilazione dei sorgenti su sistema operativo linux-based all'interno della cartella "compilato"
dotnet build --os linux --self-contained false -o ./compilato
// analogo del comando precedente per la compilazione su Windows
dotnet build --os win --self-contained false -o ./compilato
```
Per ulteriori informazioni sugli attributi di compilazione, visionare la pagina dedicata al comando [dotnet build](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build) fatta da Microsoft. 
