# CryptoPal

![CI](https://github.com/lgamorim/cryptopal/actions/workflows/ci.yml/badge.svg)

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

# Verify formatting (also enforced in CI).
dotnet format CryptoPal.slnx --verify-no-changes
```

### Continuous integration

GitHub Actions runs on every push to `master` and on pull requests targeting `master`. The workflow
builds and tests the solution on `ubuntu-latest` and `windows-latest` (Release configuration), then
verifies that `dotnet format --verify-no-changes` passes.

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

When CoinGecko is unavailable or returns an error, the command writes `{ErrorCode}: {message}` to stderr
and exits with code `1` instead of printing empty output. User-initiated cancellation (for example Ctrl+C)
writes `Operation canceled.` to stderr and exits with code `130`. Requesting multiple coins or contract
addresses fails when any requested identifier is missing from the upstream response.

## ViewerApi

`CryptoPal.ViewerApi` exposes the same data over HTTP. Configure the API key with user secrets on that
project, then run:

```sh
dotnet user-secrets set "CoinGecko:ApiKey" "<your-key>" --project src/CryptoPal.ViewerApi
dotnet run --project src/CryptoPal.ViewerApi
```

Routes mirror the console commands: `/prices`, `/token-prices`, `/historical-market-data`,
`/coins/{coin}`, and `/coins/{coin}/developer-data`.

### API documentation

- OpenAPI document: `GET /openapi/v1.json`
- Scalar UI (Development only): `/scalar`
- Health check: `GET /health`

### Error responses

On upstream failure the API returns [RFC 7807 ProblemDetails](https://www.rfc-editor.org/rfc/rfc7807)
instead of `200 OK` with empty data:

| Situation | HTTP status | `title` (error code) |
|-----------|-------------|----------------------|
| Resource not found upstream | `404` | `NotFound` |
| CoinGecko rate limit | `429` | `RateLimited` |
| Upstream request timed out | `504` | `RequestTimedOut` |
| Other upstream/transport failure | `502` | `UpstreamUnavailable` |
| Response could not be mapped | `500` | `ResponseMappingFailed` |

The `detail` field contains a short human-readable message.

Invalid JSON or malformed wire format from CoinGecko is classified as `UpstreamUnavailable` (`502`).
Valid JSON that fails domain mapping (for example an out-of-range timestamp) is classified as
`ResponseMappingFailed` (`500`). Client disconnect or cancellation does not produce ProblemDetails.

Requesting multiple coins or contract addresses fails with `404 NotFound` when any requested
identifier is missing from the upstream response.

## Projects

The solution is split into a thin presentation host, a core layer that owns the domain workflow,
and an isolated API client, plus a unit test project for each layer that contains logic.
Source projects live under `src/`; unit tests under `test/`.

| Project                                   | Description |
|-------------------------------------------|-------------|
| `CryptoPal.ViewerApp`                     | Console entry point and presentation host. Parses command-line arguments, configures dependency injection and logging, dispatches to the core service, and formats the results for the terminal. Holds no business logic. |
| `CryptoPal.ViewerApi`                     | Minimal REST API over the same core service. Maps HTTP routes to query objects and returns view models as JSON. |
| `CryptoPal.Core`                          | The core layer. Exposes `ICryptocurrencyService`, which accepts query objects (`GetCurrentPriceQuery`, `GetTokenPriceQuery`, `GetHistoricalMarketDataQuery`, `GetCoinDataQuery`, `GetDeveloperDataQuery`), orchestrates calls to the CoinGecko client, and maps the raw API responses into presentation-friendly view models (`CurrentPriceView`, `TokenPriceView`, `HistoricalMarketDataView`, `CoinDataView`, `DeveloperDataView`) wrapped in `ServiceResult<T>` so upstream failures propagate with stable `ServiceErrorCode` values. |
| `CryptoPal.ApiClient.CoinGecko`          | A focused, reusable client for the CoinGecko REST API. Wraps an `HttpClient` (configured via `IHttpClientFactory`), builds the request URLs, deserializes JSON responses, and translates failures into result objects. It is the only project that knows about CoinGecko's wire format and endpoints. |
| `CryptoPal.Core.UnitTests`                | Unit tests for the core layer, exercising `CryptocurrencyService`'s orchestration, response-to-view mapping, validation, and failure handling with a mocked CoinGecko client. |
| `CryptoPal.ApiClient.CoinGecko.UnitTests` | Unit tests for the CoinGecko client, verifying URL construction, JSON deserialization, and error handling against a fake `HttpMessageHandler`. |
| `CryptoPal.ViewerApp.UnitTests`           | Unit tests for the console host, verifying command parsing, output formatting, usage messages, and exit codes against a mocked `ICryptocurrencyService`. |
| `CryptoPal.ViewerApi.UnitTests`           | Unit tests for the REST endpoints, verifying route handlers delegate to `ICryptocurrencyService`, return successful view models, and map service failures to ProblemDetails status codes. |

### Dependencies

`CryptoPal.ViewerApp` → `CryptoPal.Core` → `CryptoPal.ApiClient.CoinGecko`

`CryptoPal.ViewerApi` → `CryptoPal.Core` → `CryptoPal.ApiClient.CoinGecko`

The dependency flow points inward toward the API client; lower layers never reference the layers above them.
Presentation hosts register infrastructure types in their composition root only.
