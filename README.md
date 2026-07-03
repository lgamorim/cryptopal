# CryptoPal

A console application that sources cryptocurrency market data — such as live prices across multiple
fiat currencies, live prices for tokens by contract address, historical price, market cap, and trading
volume series, detailed metadata for a single coin, and developer (repository) activity for a coin on a
given date — from the [CoinGecko](https://www.coingecko.com/en/api) public API.

## Solution

The solution uses the XML-based solution format. Open `CryptoPal.slnx` in a compatible IDE (Visual Studio
2022 17.10+, JetBrains Rider, or VS Code) or use the .NET CLI from the repository root.

## Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) or later.
- A [CoinGecko demo API key](https://www.coingecko.com/en/api/pricing) (free tier).

## Configuration

The viewer reads the CoinGecko API key from the `CoinGecko:ApiKey` configuration value, which is
supplied through [.NET user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets).
Set it once before running the app:

```sh
dotnet user-secrets set "CoinGecko:ApiKey" "<your-key>" --project src/CryptoPal.ViewerApp
```

The key is sent to CoinGecko on every request via the `x-cg-demo-api-key` header. If it is not
configured, the app exits with an error explaining how to set it.

## Building and testing

```sh
# Restore and build every project in the solution.
dotnet build CryptoPal.slnx

# Run the full unit test suite.
dotnet test CryptoPal.slnx
```

## Running the app

The viewer is a console application driven by command-line arguments. Run it through the .NET CLI by
passing the arguments after `--`:

```sh
dotnet run --project src/CryptoPal.ViewerApp -- <command> <args...>
```

It supports five commands:

### `price` — current prices

Fetches the latest price for one or more coins, each quoted in one or more currencies.

```
price <coins> <currencies>
```

| Argument       | Description                            | Example                |
|----------------|----------------------------------------|------------------------|
| `<coins>`      | Comma-separated CoinGecko coin IDs.    | `bitcoin,ethereum`     |
| `<currencies>` | Comma-separated target currency codes. | `eur,usd`              |

```sh
dotnet run --project src/CryptoPal.ViewerApp -- price bitcoin,ethereum eur,usd
```

### `token` — current prices by token

Fetches the latest price for one or more tokens, identified by their contract address on a given asset
platform, each quoted in one or more currencies.

```
token <platform> <addresses> <currencies>
```

| Argument       | Description                                        | Example                                      |
|----------------|----------------------------------------------------|----------------------------------------------|
| `<platform>`   | A single CoinGecko asset platform ID.              | `ethereum`                                   |
| `<addresses>`  | Comma-separated token contract addresses.          | `0xdac17f958d2ee523a2206206994597c13d831ec7` |
| `<currencies>` | Comma-separated target currency codes.             | `eur,usd`                                    |

```sh
dotnet run --project src/CryptoPal.ViewerApp -- token ethereum 0xdac17f958d2ee523a2206206994597c13d831ec7 eur,usd
```

### `history` — historical market data

Fetches historical price, market cap, and trading volume data for a single coin over a number of days.
(The console output currently lists the daily price series.)

```
history <coin> <currency> <days>
```

| Argument     | Description                                      | Example   |
|--------------|--------------------------------------------------|-----------|
| `<coin>`     | A single CoinGecko coin ID.                      | `bitcoin` |
| `<currency>` | A single target currency code.                   | `eur`     |
| `<days>`     | Number of days of history to retrieve (integer). | `7`       |

```sh
dotnet run --project src/CryptoPal.ViewerApp -- history bitcoin eur 7
```

### `coin` — coin data by ID

Fetches detailed data for a single coin: its symbol, name, English description, image, 24-hour price
change percentage, and per-currency market snapshots (current price, market cap, and trading volume).
(The console output currently lists the identifier, the 24-hour change, and the current price per
currency.)

```
coin <id>
```

| Argument | Description                 | Example   |
|----------|-----------------------------|-----------|
| `<id>`   | A single CoinGecko coin ID. | `bitcoin` |

```sh
dotnet run --project src/CryptoPal.ViewerApp -- coin bitcoin
```

### `developer` — developer data by ID

Fetches developer (source-repository) activity for a single coin on a specific historical date: forks,
stars, subscribers, total and closed issues, merged pull requests and contributors, the code additions
and deletions over the last four weeks, and the four-week commit count.

```
developer <id> <date>
```

| Argument | Description                                       | Example      |
|----------|---------------------------------------------------|--------------|
| `<id>`   | A single CoinGecko coin ID.                       | `bitcoin`    |
| `<date>` | The snapshot date in `dd-mm-yyyy` format.         | `30-12-2025` |

```sh
dotnet run --project src/CryptoPal.ViewerApp -- developer bitcoin 30-12-2025
```

Coin and currency values are trimmed, and any empty entries are ignored. Invoking the app with no
arguments, an unknown command, or the wrong number/type of arguments prints a usage message and exits
with a non-zero status code.

## Projects

The solution is split into a thin presentation host, a core layer that owns the domain workflow,
and an isolated API client, plus a unit test project for each layer that contains logic.

| Project                                   | Description |
|-------------------------------------------|-------------|
| `CryptoPal.ViewerApp`                     | Console entry point and presentation host. Parses command-line arguments, configures dependency injection and logging, dispatches to the core service, and formats the results for the terminal. Holds no business logic. |
| `CryptoPal.Core`                          | The core layer. Exposes `ICryptocurrencyService`, which accepts query objects (`GetCurrentPriceQuery`, `GetTokenPriceQuery`, `GetHistoricalMarketDataQuery`, `GetCoinDataQuery`, `GetDeveloperDataQuery`), orchestrates calls to the CoinGecko client, and maps the raw API responses into presentation-friendly view models (`CurrentPriceView`, `TokenPriceView`, `HistoricalMarketDataView`, `CoinDataView`, `DeveloperDataView`) using domain types such as `Price`, `ContractPrice`, `DatedValue`, and `CoinMarketSnapshot`. |
| `CryptoPal.ApiClient.CoinGecko`          | A focused, reusable client for the CoinGecko REST API. Wraps an `HttpClient` (configured via `IHttpClientFactory`), builds the request URLs, deserializes JSON responses, and translates failures into result objects. It is the only project that knows about CoinGecko's wire format and endpoints. |
| `CryptoPal.Core.UnitTests`                | Unit tests for the core layer, exercising `CryptocurrencyService`'s orchestration, response-to-view mapping, validation, and failure handling with a mocked CoinGecko client. |
| `CryptoPal.ApiClient.CoinGecko.UnitTests` | Unit tests for the CoinGecko client, verifying URL construction, JSON deserialization, and error handling against a fake `HttpMessageHandler`. |
| `CryptoPal.ViewerApp.UnitTests`           | Unit tests for the console host, verifying command parsing, output formatting, usage messages, and exit codes against a mocked `ICryptocurrencyService`. |

### Dependencies

`CryptoPal.ViewerApp` → `CryptoPal.Core` → `CryptoPal.ApiClient.CoinGecko`. The dependency flow
points inward toward the API client; lower layers never reference the layers above them.
