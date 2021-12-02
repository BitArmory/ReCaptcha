@echo off
cls

SET BUILDER=Builder

dotnet tool restore

dotnet fake run %BUILDER%\build.fsx target %1