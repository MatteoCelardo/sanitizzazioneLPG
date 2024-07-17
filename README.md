# Sanitizzatore LPG - Neo4J

## Indice 
- [Descrizione](#Descrizione)
- [Librerie e framework impiegati](#Librerie-e-framework-impiegati)
- [Requisiti e setup](#Requisiti-e-setup)
- [Manuale](#Manuale)
- [Sviluppi](#Sviluppi)
## Descrizione

Questo progetto si pone lo scopo di creare un software che, fornite informazioni su cosa sia sensibile all'interno di un LPG Neo4J, permetta di sanitizzare la base di dati, andando a creare autonomamente le query da inviare al DBMS e velocizzando le operazioni richieste a un amministratore di rete che avesse la necessità di condividere il contenuto di un database in forma anonima.
L'applicativo richiede all'amministratore di creare un file JSON contenete le specifiche che permetteranno di creare le query di sanitizzazione tramite la definizioni di quali nodi e/o relazioni rappresentino in toto o in parte un dato sensibile.

## Librerie e framework impiegati

Il software è stato sviluppato interamente in C\# e fa uso delle seguenti tecnologie:
- libreria [Neo4jClient](https://github.com/DotNet4Neo4j/Neo4jClient): è stata utilizzata per gestire la comunicazione col database Neo4J, nonché per la generazione delle query impiegate nella sanitizzazione;
- framework [Avalonia UI](https://avaloniaui.net/): lo sviluppo dell'intero applicativo è stato condotto secondo pattern architetturale MVVM per permette l'uso di questo framework nella costruzione delle interfacce grafiche, oltre che per conferire maggiore manutenibilità al codice;
- framework [Newtonsoft](https://www.newtonsoft.com/json): questo framework è stato utilizzato per effettuare il parsing del file JSON ricevuto come input.


## Requisiti e setup

L'unico requisito richiesto dal progetto è quello di avere installato correttamente la piattaforma dotnet al fine di poter compilare il sorgente in maniera adatta al proprio sistema operativo.
Per maggiori informazioni condurre il processo d'installazione di dotnet, consultare la [guida fornita da Microsoft](https://dotnet.microsoft.com/en-us/download).
Di seguito due esempi per la compilazione del sorgente:
```C
// compilazione dei sorgenti su sistema operativo linux-based all'interno della cartella "compilato"
dotnet build --os linux --self-contained false -o ./compilato
// analogo del comando precedente per la compilazione su Windows
dotnet build --os win --self-contained false -o ./compilato
```
Per ulteriori informazioni sugli attributi di compilazione, visionare la pagina dedicata al comando [dotnet build](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build).

Fatto ciò, sarà possibile utilizzare l'eseguibile appena compilato per avviare il programma. 

## Manuale

Per informazioni sull'utilizzo del software, visionare i capitoli 3 e 4 della tesi intitolata _PROTEZIONE DI INFORMAZIONI SENSIBILI IN BASI DI DATI A GRAFO_, realizzata da Celardo Matteo a conclusione di una laurea triennale in Sicurezza dei Sistemi e delle Reti presso l'Università degli Studi di Milano. Per maggiori informazioni su come reperire la tesi, riferirsi al [seguente link](https://www.sba.unimi.it/Tesi/49.html).
 
## Sviluppi

Come definito all'interno della tesi sopra citata, i principali punti individuati su cui effettuare sviluppo sono:
- introduzione di nuovi metodi per la sanitizzazione oltre alla cancellazione;
- creazione di un'ulteriore interfaccia grafica per la creazione del file JSON tramite un'interazione più _user friendly_, attraverso, ad esempio, l'impiego di bottoni e caselle di testo; 
- introduzione di metriche per la sanitizzazione in automatico delle catene, richiedendo solo di specificare quali nodi e relazioni ne facciano parte;
- sviluppo di un sistema di machine learning con delle metriche per la definizione di cosa rappresenti informazioni sensibili e cosa non lo sia.

Sono stati inoltre individuati alcuni punti su cui cercare efficientamento del codice esistente:
- studio più approfondito della sintassi cypher per la creazione di query con minor tempo di esecuzione;
- utilizzo di batching per spedire le query da eseguire al database;
- cambiare la libreria usata per interagire col database al fine di avere un implementazione più snella e rapida delle funzioni che realizzano la sanitizzazione delle catene.
