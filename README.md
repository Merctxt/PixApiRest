# PIX API REST - .NET

![.NET](https://img.shields.io/badge/.NET-9-blue)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv2-blue.svg)](https://www.gnu.org/licenses/gpl-2.0)

REST API for generating PIX payments following the official EMV QR Code standard defined by Banco Central do Brasil.

The API generates valid PIX payloads and QR Codes for static payments — fully dynamic, no database required.

**Production API (Scalar UI):**  
https://pix.giovannidev.com/scalar/

---

### Environment variables (.env)

```env
SERVER_ADDRESS=0.0.0.0
SERVER_PORT=8080
```

### Running the Project

```bash
# Restore dependencies
dotnet restore

# Run the project
dotnet run
```

The API will be available at `http://localhost:8080`.  
The interactive docs (Scalar UI) will be at `http://localhost:8080/scalar/v1`.

---

## Endpoints

### `POST /pix/payment`

Generates a PIX EMV payload from payment data.

**Request body:**

```json
{
  "amount": 100.00,
  "pixKey": "email@exemplo.com",
  "receiverName": "Venus Store",
  "receiverCity": "SAO PAULO",
  "merchantCategoryCode": "0000"
}
```

> `receiverCity` and `merchantCategoryCode` are optional — defaults are `"SAO PAULO"` and `"0000"`.

**Response:**

```json
{
  "payload": "00020126580014BR.GOV.BCB.PIX..."
}
```

---

### `POST /pix/qrcode`

Generates a PNG QR Code image from a PIX EMV payload.

**Request body:**

```json
{
  "payload": "00020126580014BR.GOV.BCB.PIX..."
}
```

**Response:** `image/png` binary

---

## Docker

```bash
docker build -t pix-api-rest .
docker run -p 8080:8080 --env-file .env pix-api-rest
```

---

## EMV PIX Standard

The API generates PIX payloads according to the official EMV QR Code specification defined by Banco Central do Brasil.

| ID | Field | Description |
|----|-------|-------------|
| 00 | Payload Format Indicator | Always "01" |
| 26 | Merchant Account Information | Contains the PIX key |
| 52 | Merchant Category Code | MCC code |
| 53 | Transaction Currency | "986" (BRL) |
| 54 | Transaction Amount | Payment amount |
| 58 | Country Code | "BR" |
| 59 | Merchant Name | Receiver name (max 25 chars) |
| 60 | Merchant City | Receiver city (max 15 chars) |
| 62 | Additional Data Field | Contains TXID |
| 63 | CRC16 | CRC16-CCITT-FALSE checksum |
