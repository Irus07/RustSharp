# Скрипт для регенерации классов ANTLR4 лексера и парсера
$jar = "$PSScriptRoot/antlr-4.9.3-complete.jar"
$outDir = "$PSScriptRoot/../src/RustSharp/Frontend/Generated"
$grammarDir = "$PSScriptRoot/../src/RustSharp/Grammar"

if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null
}

Write-Host "Генерация классов ANTLR4 C#..." -ForegroundColor Cyan
java -jar $jar -Dlanguage=CSharp -package RustSharp.Core.Frontend.Generated -visitor -listener -o $outDir "$grammarDir/RustSharpLexer.g4" "$grammarDir/RustSharpParser.g4"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Классы ANTLR успешно сгенерированы в $outDir" -ForegroundColor Green
} else {
    Write-Host "Ошибка при генерации грамматики!" -ForegroundColor Red
}
