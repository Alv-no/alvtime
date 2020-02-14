FROM mcr.microsoft.com/mssql/server:2017-latest

ARG SA_PASSWORD
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=AlvTimeTestErMoro32
ENV MSSQL_PID=Developer
ENV MSSQL_TCP_PORT=1433 

WORKDIR /src
COPY ./ /scripts/

EXPOSE 1433 

RUN (/opt/mssql/bin/sqlservr --accept-eula & ) | grep -q "Service Broker manager has started" && sleep 5s && (for foo in /scripts/*.sql;do /opt/mssql-tools/bin/sqlcmd -S127.0.0.1 -Usa -P${SA_PASSWORD}  -i$foo;done)