using DevHub.Application.UseCases.Projects;
using DevHub.Domain.Enums;

namespace DevHub.Application.Tests.UseCases;

public class DetectProjectsUseCaseTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly DetectProjectsUseCase _useCase;

    public DetectProjectsUseCaseTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "DevHub_Test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
        _useCase = new DetectProjectsUseCase();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, true);
    }

    [Fact]
    public async Task ExecuteAsync_NonExistentPath_ReturnsEmpty()
    {
        var result = await _useCase.ExecuteAsync("Z:\\nonexistent");

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyDirectory_ReturnsEmpty()
    {
        var result = await _useCase.ExecuteAsync(_tempRoot);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_DirectoryWithCsProject_DetectsCSharp()
    {
        var projectDir = Path.Combine(_tempRoot, "MyProject");
        Directory.CreateDirectory(projectDir);
        File.WriteAllText(Path.Combine(projectDir, "Program.cs"), "class Program {}");

        var result = await _useCase.ExecuteAsync(_tempRoot);

        Assert.Single(result);
        Assert.Equal("MyProject", result[0].Name);
        Assert.Equal(ProgrammingLanguage.CSharp, result[0].Language);
    }

    [Fact]
    public async Task ExecuteAsync_DirectoryWithPythonFile_DetectsPython()
    {
        var projectDir = Path.Combine(_tempRoot, "PyProject");
        Directory.CreateDirectory(projectDir);
        File.WriteAllText(Path.Combine(projectDir, "main.py"), "print('hello')");

        var result = await _useCase.ExecuteAsync(_tempRoot);

        Assert.Single(result);
        Assert.Equal(ProgrammingLanguage.Python, result[0].Language);
    }

    [Fact]
    public async Task ExecuteAsync_DirectoryWithJsFile_DetectsJavaScript()
    {
        var projectDir = Path.Combine(_tempRoot, "JsProject");
        Directory.CreateDirectory(projectDir);
        File.WriteAllText(Path.Combine(projectDir, "index.js"), "console.log('hi')");

        var result = await _useCase.ExecuteAsync(_tempRoot);

        Assert.Single(result);
        Assert.Equal(ProgrammingLanguage.JavaScript, result[0].Language);
    }

    [Fact]
    public async Task ExecuteAsync_DirectoryWithTsFile_DetectsTypeScript()
    {
        var projectDir = Path.Combine(_tempRoot, "TsProject");
        Directory.CreateDirectory(projectDir);
        File.WriteAllText(Path.Combine(projectDir, "app.ts"), "const x = 1;");

        var result = await _useCase.ExecuteAsync(_tempRoot);

        Assert.Single(result);
        Assert.Equal(ProgrammingLanguage.TypeScript, result[0].Language);
    }

    [Fact]
    public async Task ExecuteAsync_DirectoryWithOnlyIgnoredFiles_ReturnsEmpty()
    {
        var projectDir = Path.Combine(_tempRoot, "ReadmeProject");
        Directory.CreateDirectory(projectDir);
        File.WriteAllText(Path.Combine(projectDir, "README.md"), "# Hello");
        File.WriteAllText(Path.Combine(projectDir, ".gitignore"), "bin/");

        var result = await _useCase.ExecuteAsync(_tempRoot);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsNodeModules()
    {
        var rootDir = Path.Combine(_tempRoot, "Root");
        Directory.CreateDirectory(rootDir);

        var nmDir = Path.Combine(rootDir, "node_modules", "pkg");
        Directory.CreateDirectory(nmDir);
        File.WriteAllText(Path.Combine(nmDir, "index.js"), "module.exports = {};");

        var result = await _useCase.ExecuteAsync(rootDir);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsExcludedFolders()
    {
        var rootDir = Path.Combine(_tempRoot, "Root");
        Directory.CreateDirectory(rootDir);

        var excludedFolders = new[] { "bin", "obj", ".git", "__pycache__", "target", ".next", "dist", "build" };
        foreach (var folder in excludedFolders)
        {
            var dir = Path.Combine(rootDir, folder);
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "test.cs"), "class T {}");
        }

        var result = await _useCase.ExecuteAsync(rootDir);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleProjects_DetectsAll()
    {
        var p1 = Path.Combine(_tempRoot, "CSharpApp");
        var p2 = Path.Combine(_tempRoot, "PythonApp");
        Directory.CreateDirectory(p1);
        Directory.CreateDirectory(p2);
        File.WriteAllText(Path.Combine(p1, "Program.cs"), "class P {}");
        File.WriteAllText(Path.Combine(p2, "app.py"), "print(1)");

        var result = await _useCase.ExecuteAsync(_tempRoot);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ExecuteAsync_NestedProject_DetectsLanguage()
    {
        var nestedDir = Path.Combine(_tempRoot, "Outer", "Inner");
        Directory.CreateDirectory(nestedDir);
        File.WriteAllText(Path.Combine(nestedDir, "main.go"), "package main");

        var result = await _useCase.ExecuteAsync(_tempRoot);

        Assert.Single(result);
        Assert.Equal(ProgrammingLanguage.Go, result[0].Language);
    }

    [Fact]
    public async Task ExecuteAsync_DetectsRust()
    {
        var projectDir = Path.Combine(_tempRoot, "RustApp");
        Directory.CreateDirectory(projectDir);
        File.WriteAllText(Path.Combine(projectDir, "main.rs"), "fn main() {}");

        var result = await _useCase.ExecuteAsync(_tempRoot);

        Assert.Single(result);
        Assert.Equal(ProgrammingLanguage.Rust, result[0].Language);
    }

    [Fact]
    public async Task ExecuteAsync_DetectsJava()
    {
        var projectDir = Path.Combine(_tempRoot, "JavaApp");
        Directory.CreateDirectory(projectDir);
        File.WriteAllText(Path.Combine(projectDir, "Main.java"), "class Main {}");

        var result = await _useCase.ExecuteAsync(_tempRoot);

        Assert.Single(result);
        Assert.Equal(ProgrammingLanguage.Java, result[0].Language);
    }
}
