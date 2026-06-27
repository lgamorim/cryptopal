# CryptoPal

A console application that sources cryptocurrency market data — such as live prices across multiple
fiat currencies and historical price, market cap, and trading volume series — from the
[CoinGecko](https://www.coingecko.com/en/api) public API.

## Solution

The solution uses the XML-based solution format. Open `CryptoPal.slnx` in a compatible IDE (Visual Studio
2022 17.10+, JetBrains Rider, or VS Code) or use the .NET CLI from the repository root.

## Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) or later.

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

It supports two commands:

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

Coin and currency values are trimmed, and any empty entries are ignored. Invoking the app with no
arguments, an unknown command, or the wrong number/type of arguments prints a usage message and exits
with a non-zero status code.

## Projects

The solution is split into a thin presentation host, an application layer that owns the domain workflow,
and an isolated API client, plus a unit test project for each layer that contains logic.

| Project                                   | Description |
|-------------------------------------------|-------------|
| `CryptoPal.ViewerApp`                     | Console entry point and presentation host. Parses command-line arguments, configures dependency injection and logging, dispatches to the application service, and formats the results for the terminal. Holds no business logic. |
| `CryptoPal.Application`                   | The application layer. Exposes `ICryptocurrencyService`, which accepts query objects (`GetCurrentPriceQuery`, `GetHistoricalMarketDataQuery`), orchestrates calls to the CoinGecko client, and maps the raw API responses into presentation-friendly view models (`CurrentPriceView`, `HistoricalMarketDataView`) using domain types such as `Price` and `DatedValue`. |
| `CryptoPal.ApiClient.CoinGecko`          | A focused, reusable client for the CoinGecko REST API. Wraps an `HttpClient` (configured via `IHttpClientFactory`), builds the request URLs, deserializes JSON responses, and translates failures into result objects. It is the only project that knows about CoinGecko's wire format and endpoints. |
| `CryptoPal.Application.UnitTests`         | Unit tests for the application layer, exercising `CryptocurrencyService`'s orchestration, response-to-view mapping, validation, and failure handling with a mocked CoinGecko client. |
| `CryptoPal.ApiClient.CoinGecko.UnitTests` | Unit tests for the CoinGecko client, verifying URL construction, JSON deserialization, and error handling against a fake `HttpMessageHandler`. |

### Dependencies

`CryptoPal.ViewerApp` → `CryptoPal.Application` → `CryptoPal.ApiClient.CoinGecko`. The dependency flow
points inward toward the API client; lower layers never reference the layers above them.
