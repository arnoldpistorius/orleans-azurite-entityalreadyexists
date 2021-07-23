This repository contains a project that reproduces Orleans Azure Table Storage error on Azurite when reading and writing data.

Scenario:

- Have a clean Azurite instance running (Table storage)
- Run the project
- Run the project again

An EntityAlreadyExists error will occur. This doesn't happen on a 'real' Table Storage in Azure.

I have debugged the Orleans Azure Table Storage implementation and it appears that the ETag is not being populated from the GET record request from Azurite, while it is on Azure Table Storage. Altough there isn't much difference in the response on the GET request (got these from Fiddler:)

## Azure
With following request to Azure, the ETag property will get populated, however I have no clue how that's done (except from that I can imagine it's being populated from Timestamp)

Request:
```txt
GET https://REDACTED.table.core.windows.net/OrleansGrainState?$filter=%28PartitionKey%20eq%20%27Orleans_GrainReference%3D0000000000000000000000000000000006ffffff9834267b%2Bhelloworld%27%29%20and%20%28RowKey%20eq%20%27Demo.MyGrainWithState%2CDemo.state%27%29 HTTP/1.1
Host: REDACTED.table.core.windows.net
Accept-Charset: UTF-8
MaxDataServiceVersion: 3.0;NetFx
Accept: application/json; odata=nometadata
DataServiceVersion: 3.0;
x-ms-client-request-id: efaa69c6-465e-4784-807b-c6ed408d310a
User-Agent: Azure-Cosmos-Table/1.0.5 (.NET CLR 5.0.8; Win32NT 10.0.19043.0)
x-ms-version: 2017-07-29
x-ms-date: Fri, 23 Jul 2021 14:13:53 GMT
Authorization: SharedKey REDACTED

HTTP/1.1 200 OK
Cache-Control: no-cache
Content-Type: application/json;odata=nometadata;streaming=true;charset=utf-8
Server: Windows-Azure-Table/1.0 Microsoft-HTTPAPI/2.0
x-ms-request-id: 2428b426-1002-0064-3bcc-7fc757000000
x-ms-version: 2017-07-29
X-Content-Type-Options: nosniff
Date: Fri, 23 Jul 2021 14:13:53 GMT
Content-Length: 278
```

Response:
```
{"value":[{"PartitionKey":"Orleans_GrainReference=0000000000000000000000000000000006ffffff9834267b+helloworld","RowKey":"Demo.MyGrainWithState,Demo.state","Timestamp":"2021-07-23T14:13:39.4157924Z","Data":"ZAECvAEbbCAARGVtby5NeUdyYWluV2l0aFN0YXRlK1N0YXRlLERlbW8FAAAASGVsbG8="}]}
```

## Azurite
With this request to Azurite, the ETag property is not being populated. Altough the Timestamp field is present. However the order of fields differs (which shouldn't matter in my opinion).

Request:
```txt
GET http://127.0.0.1:10002/devstoreaccount1/OrleansGrainState?$filter=%28PartitionKey%20eq%20%27Orleans_GrainReference%3D0000000000000000000000000000000006ffffff9834267b%2Bhelloworld%27%29%20and%20%28RowKey%20eq%20%27Demo.MyGrainWithState%2CDemo.state%27%29 HTTP/1.1
Host: 127.0.0.1:10002
Accept-Charset: UTF-8
MaxDataServiceVersion: 3.0;NetFx
Accept: application/json; odata=nometadata
DataServiceVersion: 3.0;
x-ms-client-request-id: 02377498-fb3e-426d-9d47-60f9f3721c2d
User-Agent: Azure-Cosmos-Table/1.0.5 (.NET CLR 5.0.8; Win32NT 10.0.19043.0)
x-ms-version: 2017-07-29
x-ms-date: Fri, 23 Jul 2021 14:12:30 GMT
Authorization: SharedKey devstoreaccount1:onl05zL567Lm7yg0AjQRFrBZbAKRnFKe0m1b35cF4u0=
```

```txt
HTTP/1.1 200 OK
Server: Azurite-Table/3.13.1
content-type: application/json;odata=nometadata
x-ms-client-request-id: 02377498-fb3e-426d-9d47-60f9f3721c2d
x-ms-request-id: ae68d175-88ef-4c05-abd6-f70caffe1a87
x-ms-version: 2020-08-04
date: Fri, 23 Jul 2021 14:12:29 GMT
Connection: keep-alive
Keep-Alive: timeout=5
Content-Length: 278

{"value":[{"PartitionKey":"Orleans_GrainReference=0000000000000000000000000000000006ffffff9834267b+helloworld","RowKey":"Demo.MyGrainWithState,Demo.state","Data":"ZAECvAEbbCAARGVtby5NeUdyYWluV2l0aFN0YXRlK1N0YXRlLERlbW8FAAAASGVsbG8=","Timestamp":"2021-07-23T14:12:15.0270000Z"}]}
```