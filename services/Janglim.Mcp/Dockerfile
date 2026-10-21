# Build from the repository root:
# docker build -f services/Janglim.Mcp/Dockerfile -t janglim-mcp .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy only the MCP service and its engine dependencies. The Playground and
# licensed frontend sources are not needed to build this service.
COPY src/Directory.Build.props src/Directory.Build.props
COPY src/Common/AJ.Common/ src/Common/AJ.Common/
COPY src/FrontEnd/Parse/ src/FrontEnd/Parse/
COPY src/FrontEnd/Parse.FrontEnd/ src/FrontEnd/Parse.FrontEnd/
COPY src/FrontEnd/Parse.FrontEnd.Grammars/ src/FrontEnd/Parse.FrontEnd.Grammars/
COPY src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/ src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/
COPY src/FrontEnd/Parse.FrontEnd.Parsers/ src/FrontEnd/Parse.FrontEnd.Parsers/
COPY src/FrontEnd/Parse.FrontEnd.ErrorHandler/ src/FrontEnd/Parse.FrontEnd.ErrorHandler/
COPY src/FrontEnd/Parse.FrontEnd.Support/ src/FrontEnd/Parse.FrontEnd.Support/
COPY grammars/c/c11-core.ebnf grammars/c/c11-core.ebnf
COPY grammars/javascript/ grammars/javascript/
COPY services/Janglim.Mcp/ services/Janglim.Mcp/

RUN dotnet publish services/Janglim.Mcp/Janglim.Mcp.csproj \
    --configuration Release --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish ./
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Janglim.Mcp.dll"]
