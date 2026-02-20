# PIX API REST - .NET

![.NET](https://img.shields.io/badge/.NET-9-blue)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14+-blue)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14+-blue)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv2-blue.svg)](https://www.gnu.org/licenses/gpl-2.0)


REST API for generating PIX payments following the official EMV QR Code standard defined by Banco Central do Brasil.

The API generates valid PIX payloads and QR Codes for static payments, ready to be consumed by banking applications and financial systems.

**Production API (Swagger UI):**  
https://pix.giovannidev.com/index.html


### environment variables (.env)

```env
DATABASE_URL=postgresql://user:password@host:port/database
SERVER_ADDRESS=0.0.0.0
SERVER_PORT=8080
PIX_RECEIVER_CITY=SAO PAULO
LIMIT_TIME_PIX=3600
LIMIT_REQUESTS_DAY_BY_IP=20
```

- `LIMIT_TIME_PIX`: Maximum time (in seconds) that a PIX transaction remains at the database. After this period, it is automatically removed. Default: 3600 (1 hour).
- `LIMIT_REQUESTS_DAY_BY_IP`: Maximum number of payment creation requests per IP per day. Default: 20.

### Running the Project

```bash
# Restore dependencies
dotnet restore

# Run the project
dotnet run
```

The API will be available at `http://localhost:8080` with the Swagger documentation at the root.



### payment status

- `PENDING` - Pendente
- `APPROVED` - Aprovado
- `CANCELLED` - Cancelado
- `EXPIRED` - Expirado

### Key Types PIX

- `CPF`
- `CNPJ`
- `EMAIL`
- `PHONE`
- `RANDOM`

## Docker

```bash
docker build -t pix-api-rest .
docker run -p 8080:8080 --env-file .env pix-api-rest
```

## EMV PIX Standard

The API generates PIX payloads according to the official EMV QR Code specification defined by Banco Central do Brasil.

| ID | Campo | Descrição |
|----|-------|-----------|
| 00 | Payload Format Indicator | Sempre "01" |
| 26 | Merchant Account Information | Contém a chave PIX |
| 52 | Merchant Category Code | Código MCC |
| 53 | Transaction Currency | "986" (BRL) |
| 54 | Transaction Amount | Valor do pagamento |
| 58 | Country Code | "BR" |
| 59 | Merchant Name | Nome do recebedor (máx. 25 chars) |
| 60 | Merchant City | Cidade do recebedor (máx. 15 chars) |
| 62 | Additional Data Field | Contém o TXID |
| 63 | CRC16 | Checksum CRC16-CCITT-FALSE |