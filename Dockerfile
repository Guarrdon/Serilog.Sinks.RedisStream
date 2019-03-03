FROM microsoft/dotnet:2.2-sdk
WORKDIR /code

# copy csproj and restore as distinct layers
COPY src/SeriLog.Sinks.RedisStream/*.cs* ./src/SeriLog.Sinks.RedisStream/
COPY example/SeriLog.Sinks.RedisStream.Example/*.cs* ./example/SeriLog.Sinks.RedisStream.Example/
RUN dotnet restore ./example/SeriLog.Sinks.RedisStream.Example

WORKDIR /code/example/SeriLog.Sinks.RedisStream.Example/

RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "run"]