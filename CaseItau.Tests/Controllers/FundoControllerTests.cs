using Bogus;
using CaseItau.Application.DTOs;
using CaseItau.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CaseItau.API.Controllers.UnitTests;


/// <summary>
/// Unit tests for the <see cref="FundoController"/> class.
/// </summary>
public class FundoControllerTests
{
    private readonly Mock<IFundoService> _fundoServiceMock;
    private readonly Mock<ILogger<FundoController>> _loggerMock;
    private readonly FundoController _controller;
    private readonly Faker _faker;
    private readonly Faker<CreateFundoDto> _createFundoDtoFaker;

    public FundoControllerTests()
    {
        _fundoServiceMock = new Mock<IFundoService>();
        _loggerMock = new Mock<ILogger<FundoController>>();
        _controller = new FundoController(_fundoServiceMock.Object, _loggerMock.Object);
        _faker = new Faker();
        _createFundoDtoFaker = new Faker<CreateFundoDto>();
    }

    /// <summary>
    /// Tests that Delete returns Ok when the fund is successfully deleted.
    /// Input: Valid codigo, DeleteAsync returns true.
    /// Expected: Returns OkResult and logs appropriate messages.
    /// </summary>
    [Fact]
    public async Task Delete_WhenFundExistsAndDeletedSuccessfully_ReturnsOk()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        _fundoServiceMock
            .Setup(s => s.DeleteAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        IActionResult result = await _controller.Delete(codigo, CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(s => s.DeleteAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Deleting fund with codigo '{codigo}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fund with codigo '{codigo}' deleted successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Delete returns NotFound when the fund does not exist.
    /// Input: Valid codigo, DeleteAsync returns false.
    /// Expected: Returns NotFoundResult and logs warning message.
    /// </summary>
    [Fact]
    public async Task Delete_WhenFundDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        _fundoServiceMock
            .Setup(s => s.DeleteAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        IActionResult result = await _controller.Delete(codigo, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _fundoServiceMock.Verify(s => s.DeleteAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Deleting fund with codigo '{codigo}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fund with codigo '{codigo}' not found for deletion")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Delete passes the correct CancellationToken to the service.
    /// Input: Valid codigo with a custom CancellationToken.
    /// Expected: DeleteAsync is called with the provided token.
    /// </summary>
    [Fact]
    public async Task Delete_PassesCancellationTokenToService()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;
        _fundoServiceMock
            .Setup(s => s.DeleteAsync(codigo, token))
            .ReturnsAsync(true);

        // Act
        await _controller.Delete(codigo, token);

        // Assert
        _fundoServiceMock.Verify(s => s.DeleteAsync(codigo, token), Times.Once);
    }

    /// <summary>
    /// Tests Delete with various edge case string values for codigo parameter.
    /// Input: Empty string, whitespace, very long string, special characters.
    /// Expected: Service is called with the exact codigo provided.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("ITAUTESTE01")]
    [InlineData("a")]
    [InlineData("ABC123!@#$%^&*()")]
    [InlineData("código-com-acentos-é-ã-õ")]
    public async Task Delete_WithVariousCodigoValues_CallsServiceWithProvidedCodigo(string codigo)
    {
        // Arrange
        _fundoServiceMock
            .Setup(s => s.DeleteAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _controller.Delete(codigo, CancellationToken.None);

        // Assert
        _fundoServiceMock.Verify(s => s.DeleteAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests Delete with a very long codigo string.
    /// Input: String with 10000 characters.
    /// Expected: Service is called with the entire string.
    /// </summary>
    [Fact]
    public async Task Delete_WithVeryLongCodigo_CallsServiceWithProvidedCodigo()
    {
        // Arrange
        string longCodigo = new string('A', 10000);
        _fundoServiceMock
            .Setup(s => s.DeleteAsync(longCodigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _controller.Delete(longCodigo, CancellationToken.None);

        // Assert
        _fundoServiceMock.Verify(s => s.DeleteAsync(longCodigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests Delete when DeleteAsync throws an exception.
    /// Input: Valid codigo, DeleteAsync throws exception.
    /// Expected: Exception propagates to caller.
    /// </summary>
    [Fact]
    public async Task Delete_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        InvalidOperationException expectedException = new("Service error");
        _fundoServiceMock
            .Setup(s => s.DeleteAsync(codigo, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Delete(codigo, CancellationToken.None));
        Assert.Equal(expectedException.Message, actualException.Message);
    }

    /// <summary>
    /// Tests Delete with a cancelled CancellationToken.
    /// Input: Valid codigo, already cancelled token.
    /// Expected: OperationCanceledException is thrown.
    /// </summary>
    [Fact]
    public async Task Delete_WithCancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        CancellationTokenSource cts = new();
        cts.Cancel();
        CancellationToken cancelledToken = cts.Token;
        _fundoServiceMock
            .Setup(s => s.DeleteAsync(codigo, cancelledToken))
            .ThrowsAsync(new OperationCanceledException(cancelledToken));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _controller.Delete(codigo, cancelledToken));
    }

    /// <summary>
    /// Tests Delete behavior when codigo contains only control characters.
    /// Input: String with tab, newline, and carriage return characters.
    /// Expected: Service is called with the exact string provided.
    /// </summary>
    [Theory]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\t\n\r")]
    public async Task Delete_WithControlCharactersCodigo_CallsServiceWithProvidedCodigo(string codigo)
    {
        // Arrange
        _fundoServiceMock
            .Setup(s => s.DeleteAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _controller.Delete(codigo, CancellationToken.None);

        // Assert
        _fundoServiceMock.Verify(s => s.DeleteAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssets returns Ok when the fund exists and is updated successfully.
    /// Input: Valid codigo and valor.
    /// Expected: Returns OkResult and calls service with correct parameters.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssets_WithValidCodigoAndValor_WhenFundExists_ReturnsOk()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        decimal valor = _faker.Finance.Amount();
        CancellationToken cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken))
            .ReturnsAsync(true);

        // Act
        IActionResult result = await _controller.UpdateFundAssets(codigo, valor, cancellationToken);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssets returns NotFound when the fund does not exist.
    /// Input: Valid codigo and valor, but fund not found.
    /// Expected: Returns NotFoundResult.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssets_WithValidCodigoAndValor_WhenFundNotFound_ReturnsNotFound()
    {
        // Arrange
        string codigo = _faker.Random.AlphaNumeric(10);
        decimal valor = _faker.Finance.Amount();
        CancellationToken cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken))
            .ReturnsAsync(false);

        // Act
        IActionResult result = await _controller.UpdateFundAssets(codigo, valor, cancellationToken);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssets handles various string codigo edge cases correctly.
    /// Input: Edge case string values for codigo (empty, whitespace, long, special characters).
    /// Expected: Returns Ok when fund exists, passes codigo to service as-is.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")]
    [InlineData("!@#$%^&*()")]
    [InlineData("ITAU-TEST-01")]
    public async Task UpdateFundAssets_WithStringEdgeCases_WhenFundExists_ReturnsOk(string codigo)
    {
        // Arrange
        decimal valor = 1000.50m;
        CancellationToken cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken))
            .ReturnsAsync(true);

        // Act
        IActionResult result = await _controller.UpdateFundAssets(codigo, valor, cancellationToken);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssets handles various decimal valor edge cases correctly.
    /// Input: Edge case decimal values (zero, negative, min, max, positive).
    /// Expected: Returns Ok when fund exists, passes valor to service as-is.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-100.50)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(999999999.99)]
    public async Task UpdateFundAssets_WithDecimalEdgeCases_WhenFundExists_ReturnsOk(decimal valor)
    {
        // Arrange
        string codigo = "ITAUTESTE01";
        CancellationToken cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken))
            .ReturnsAsync(true);

        // Act
        IActionResult result = await _controller.UpdateFundAssets(codigo, valor, cancellationToken);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssets handles decimal.MaxValue correctly.
    /// Input: decimal.MaxValue for valor.
    /// Expected: Returns Ok when fund exists.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssets_WithMaxDecimalValue_WhenFundExists_ReturnsOk()
    {
        // Arrange
        string codigo = "ITAUTESTE01";
        decimal valor = decimal.MaxValue;
        CancellationToken cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken))
            .ReturnsAsync(true);

        // Act
        IActionResult result = await _controller.UpdateFundAssets(codigo, valor, cancellationToken);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssets handles decimal.MinValue correctly.
    /// Input: decimal.MinValue for valor.
    /// Expected: Returns Ok when fund exists.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssets_WithMinDecimalValue_WhenFundExists_ReturnsOk()
    {
        // Arrange
        string codigo = "ITAUTESTE01";
        decimal valor = decimal.MinValue;
        CancellationToken cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken))
            .ReturnsAsync(true);

        // Act
        IActionResult result = await _controller.UpdateFundAssets(codigo, valor, cancellationToken);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssets uses default CancellationToken when not provided.
    /// Input: Valid codigo and valor with default cancellation token.
    /// Expected: Returns Ok and calls service with default token.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssets_WithDefaultCancellationToken_WhenFundExists_ReturnsOk()
    {
        // Arrange
        string codigo = "ITAUTESTE01";
        decimal valor = 5000m;
        _fundoServiceMock
            .Setup(s => s.UpdateFundAssetsAsync(codigo, valor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        IActionResult result = await _controller.UpdateFundAssets(codigo, valor);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateFundAssetsAsync(codigo, valor, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssets logs information at the start of the operation.
    /// Input: Valid codigo and valor.
    /// Expected: Logs information message with codigo and valor.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssets_LogsInformationAtStart()
    {
        // Arrange
        string codigo = "ITAUTESTE01";
        decimal valor = 1500.75m;
        CancellationToken cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken))
            .ReturnsAsync(true);

        // Act
        await _controller.UpdateFundAssets(codigo, valor, cancellationToken);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Updating fund assets")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssets logs a warning when fund is not found.
    /// Input: Valid codigo and valor, but fund does not exist.
    /// Expected: Logs warning message with codigo.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssets_WhenFundNotFound_LogsWarning()
    {
        // Arrange
        string codigo = "NONEXISTENT";
        decimal valor = 1000m;
        CancellationToken cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken))
            .ReturnsAsync(false);

        // Act
        await _controller.UpdateFundAssets(codigo, valor, cancellationToken);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssets logs success information when fund is updated.
    /// Input: Valid codigo and valor, fund exists.
    /// Expected: Logs information message about successful update.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssets_WhenFundUpdated_LogsSuccessInformation()
    {
        // Arrange
        string codigo = "ITAUTESTE01";
        decimal valor = 2000m;
        CancellationToken cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken))
            .ReturnsAsync(true);

        // Act
        await _controller.UpdateFundAssets(codigo, valor, cancellationToken);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("updated successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateFundAssets propagates exceptions from the service layer.
    /// Input: Valid codigo and valor, but service throws exception.
    /// Expected: Exception is propagated to caller.
    /// </summary>
    [Fact]
    public async Task UpdateFundAssets_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        string codigo = "ITAUTESTE01";
        decimal valor = 1000m;
        CancellationToken cancellationToken = CancellationToken.None;
        Exception expectedException = new InvalidOperationException("Service error");
        _fundoServiceMock
            .Setup(s => s.UpdateFundAssetsAsync(codigo, valor, cancellationToken))
            .ThrowsAsync(expectedException);

        // Act & Assert
        Exception exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.UpdateFundAssets(codigo, valor, cancellationToken));
        Assert.Equal("Service error", exception.Message);
    }

    /// <summary>
    /// Tests that Post method successfully creates a fund and returns 201 Created
    /// with correct action, route values, and response body when given valid input.
    /// </summary>
    [Fact]
    public async Task Post_ValidDto_ReturnsCreatedAtActionWithCorrectParameters()
    {
        // Arrange
        var dto = new CreateFundoDto
        {
            Codigo = "TEST001",
            Nome = "Test Fund",
            Cnpj = "12345678901234",
            CodigoTipo = 1,
            Patrimonio = 1000000m
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Post(dto, cancellationToken);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(FundoController.Get), createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.RouteValues);
        Assert.True(createdAtActionResult.RouteValues.ContainsKey("codigo"));
        Assert.Equal(dto.Codigo, createdAtActionResult.RouteValues["codigo"]);
        Assert.Same(dto, createdAtActionResult.Value);
    }

    /// <summary>
    /// Tests that Post method calls the service CreateAsync method with the correct
    /// DTO and cancellation token parameters.
    /// </summary>
    [Fact]
    public async Task Post_ValidDto_CallsServiceCreateAsyncWithCorrectParameters()
    {
        // Arrange
        var dto = new CreateFundoDto
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10),
            Patrimonio = _faker.Random.Decimal(1000, 1000000)
        };
        var cancellationToken = new CancellationToken();
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.Post(dto, cancellationToken);

        // Assert
        _fundoServiceMock.Verify(
            s => s.CreateAsync(dto, cancellationToken),
            Times.Once);
    }

    /// <summary>
    /// Tests that Post method logs information before creating the fund
    /// with the correct codigo value.
    /// </summary>
    [Fact]
    public async Task Post_ValidDto_LogsInformationBeforeCreating()
    {
        // Arrange
        var dto = new CreateFundoDto
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10),
            Patrimonio = _faker.Random.Decimal(1000, 1000000)
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.Post(dto, cancellationToken);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Creating fund with codigo '{dto.Codigo}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Post method logs information after successfully creating the fund
    /// with the correct codigo value.
    /// </summary>
    [Fact]
    public async Task Post_ValidDto_LogsInformationAfterCreating()
    {
        // Arrange
        var dto = new CreateFundoDto
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10),
            Patrimonio = _faker.Random.Decimal(1000, 1000000)
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.Post(dto, cancellationToken);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fund with codigo '{dto.Codigo}' created successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Post method uses default cancellation token when none is provided.
    /// </summary>
    [Fact]
    public async Task Post_NoCancellationToken_UsesDefaultCancellationToken()
    {
        // Arrange
        var dto = new CreateFundoDto
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10),
            Patrimonio = _faker.Finance.Amount()
        };
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.Post(dto);

        // Assert
        _fundoServiceMock.Verify(
            s => s.CreateAsync(dto, It.Is<CancellationToken>(ct => ct == default)),
            Times.Once);
    }

    /// <summary>
    /// Tests that Post method propagates the provided cancellation token to the service layer.
    /// </summary>
    [Fact]
    public async Task Post_CancelledToken_PropagatesCancellationToService()
    {
        // Arrange
        var dto = new CreateFundoDto
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10),
            Patrimonio = _faker.Finance.Amount()
        };
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancellationToken = cts.Token;
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _controller.Post(dto, cancellationToken));
    }

    /// <summary>
    /// Tests that Post method handles special characters in Codigo property correctly,
    /// ensuring they are properly logged and returned in the response.
    /// </summary>
    [Theory]
    [InlineData("ITAU-TEST-01")]
    [InlineData("FUND_123")]
    [InlineData("TEST@FUND")]
    [InlineData("CÓDIGO#ESPECIAL")]
    public async Task Post_CodigoWithSpecialCharacters_HandlesCorrectly(string codigo)
    {
        // Arrange
        var dto = _createFundoDtoFaker.Generate();
        dto.Codigo = codigo;
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Post(dto, cancellationToken);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(codigo, createdAtActionResult.RouteValues!["codigo"]);
    }

    /// <summary>
    /// Tests that Post method handles empty string Codigo property correctly.
    /// </summary>
    [Fact]
    public async Task Post_EmptyStringCodigo_HandlesCorrectly()
    {
        // Arrange
        var dto = new CreateFundoDto
        {
            Codigo = string.Empty,
            Nome = "Test Fund",
            Cnpj = "12345678901234",
            CodigoTipo = 1,
            Patrimonio = 1000000
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Post(dto, cancellationToken);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(string.Empty, createdAtActionResult.RouteValues!["codigo"]);
    }

    /// <summary>
    /// Tests that Post method handles whitespace-only Codigo property correctly.
    /// </summary>
    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task Post_WhitespaceOnlyCodigo_HandlesCorrectly(string codigo)
    {
        // Arrange
        var dto = _createFundoDtoFaker.Generate();
        dto.Codigo = codigo;
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Post(dto, cancellationToken);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(codigo, createdAtActionResult.RouteValues!["codigo"]);
    }

    /// <summary>
    /// Tests that Post method handles very long Codigo strings correctly.
    /// </summary>
    [Fact]
    public async Task Post_VeryLongCodigo_HandlesCorrectly()
    {
        // Arrange
        var dto = new CreateFundoDto
        {
            Codigo = new string('A', 1000),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10),
            Patrimonio = _faker.Finance.Amount()
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Post(dto, cancellationToken);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(dto.Codigo, createdAtActionResult.RouteValues!["codigo"]);
    }

    /// <summary>
    /// Tests that Post method handles boundary values for CodigoTipo property correctly.
    /// </summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public async Task Post_BoundaryCodigoTipoValues_HandlesCorrectly(int codigoTipo)
    {
        // Arrange
        var dto = _createFundoDtoFaker.Generate();
        dto.CodigoTipo = codigoTipo;
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Post(dto, cancellationToken);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.NotNull(createdAtActionResult);
    }

    /// <summary>
    /// Tests that Post method handles null Patrimonio property correctly,
    /// as it is a nullable decimal.
    /// </summary>
    [Fact]
    public async Task Post_NullPatrimonio_HandlesCorrectly()
    {
        // Arrange
        var dto = new CreateFundoDto
        {
            Codigo = "TEST001",
            Nome = "Test Fund",
            Cnpj = "12345678901234",
            CodigoTipo = 1,
            Patrimonio = null
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Post(dto, cancellationToken);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.NotNull(createdAtActionResult);
        var returnedDto = Assert.IsType<CreateFundoDto>(createdAtActionResult.Value);
        Assert.Null(returnedDto.Patrimonio);
    }

    /// <summary>
    /// Tests that Post method handles boundary and special decimal values for Patrimonio property correctly.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(999999999999.99)]
    [InlineData(-999999999999.99)]
    public async Task Post_BoundaryPatrimonioValues_HandlesCorrectly(decimal patrimonio)
    {
        // Arrange
        var dto = _createFundoDtoFaker.Generate();
        dto.Patrimonio = patrimonio;
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Post(dto, cancellationToken);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedDto = Assert.IsType<CreateFundoDto>(createdAtActionResult.Value);
        Assert.Equal(patrimonio, returnedDto.Patrimonio);
    }

    /// <summary>
    /// Tests that Post method handles decimal.MaxValue for Patrimonio property correctly.
    /// </summary>
    [Fact]
    public async Task Post_MaxValuePatrimonio_HandlesCorrectly()
    {
        // Arrange
        var dto = new CreateFundoDto
        {
            Codigo = "TEST001",
            Nome = "Test Fund",
            Cnpj = "12.345.678/0001-90",
            CodigoTipo = 1,
            Patrimonio = decimal.MaxValue
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Post(dto, cancellationToken);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedDto = Assert.IsType<CreateFundoDto>(createdAtActionResult.Value);
        Assert.Equal(decimal.MaxValue, returnedDto.Patrimonio);
    }

    /// <summary>
    /// Tests that Post method handles decimal.MinValue for Patrimonio property correctly.
    /// </summary>
    [Fact]
    public async Task Post_MinValuePatrimonio_HandlesCorrectly()
    {
        // Arrange
        var dto = new CreateFundoDto
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10),
            Patrimonio = decimal.MinValue
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.CreateAsync(dto, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Post(dto, cancellationToken);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedDto = Assert.IsType<CreateFundoDto>(createdAtActionResult.Value);
        Assert.Equal(decimal.MinValue, returnedDto.Patrimonio);
    }
    private readonly Faker<FundoDto> _fundoDtoFaker;

    /// <summary>
    /// Tests that Get returns NotFoundResult when the service returns null.
    /// </summary>
    [Fact]
    public async Task Get_WhenServiceReturnsNull_ReturnsNotFound()
    {
        // Arrange
        _fundoServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<FundoDto>?)null);

        // Act
        IActionResult result = await _controller.Get();

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching all funds")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No funds found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Get returns NotFoundResult when the service returns an empty collection.
    /// </summary>
    [Fact]
    public async Task Get_WhenServiceReturnsEmptyCollection_ReturnsNotFound()
    {
        // Arrange
        _fundoServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FundoDto>());

        // Act
        IActionResult result = await _controller.Get();

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching all funds")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No funds found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Get returns OkObjectResult with data when the service returns a collection with one item.
    /// </summary>
    [Fact]
    public async Task Get_WhenServiceReturnsOneItem_ReturnsOkWithData()
    {
        // Arrange
        List<FundoDto> fundos = new List<FundoDto>
        {
            new FundoDto
            {
                Codigo = _faker.Random.AlphaNumeric(10),
                Nome = _faker.Company.CompanyName(),
                Cnpj = _faker.Random.Replace("##.###.###/####-##"),
                CodigoTipo = _faker.Random.Int(1, 10),
                NomeTipo = _faker.Random.Word(),
                Patrimonio = _faker.Finance.Amount()
            }
        };
        _fundoServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundos);

        // Act
        IActionResult result = await _controller.Get();

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<FundoDto> returnedFundos = Assert.IsAssignableFrom<IEnumerable<FundoDto>>(okResult.Value);
        Assert.Single(returnedFundos);
        Assert.Equal(fundos.First().Codigo, returnedFundos.First().Codigo);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching all funds")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that Get returns OkObjectResult with data when the service returns a collection with multiple items.
    /// </summary>
    [Fact]
    public async Task Get_WhenServiceReturnsMultipleItems_ReturnsOkWithData()
    {
        // Arrange
        List<FundoDto> fundos = Enumerable.Range(0, 5).Select(_ => new FundoDto
        {
            Codigo = _faker.Random.AlphaNumeric(10),
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10),
            NomeTipo = _faker.Random.Word(),
            Patrimonio = _faker.Random.Decimal(0, 1000000)
        }).ToList();
        _fundoServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundos);

        // Act
        IActionResult result = await _controller.Get();

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<FundoDto> returnedFundos = Assert.IsAssignableFrom<IEnumerable<FundoDto>>(okResult.Value);
        Assert.Equal(5, returnedFundos.Count());
        Assert.Equal(fundos, returnedFundos);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching all funds")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Get passes the provided CancellationToken to the service.
    /// </summary>
    [Fact]
    public async Task Get_WithProvidedCancellationToken_PassesTokenToService()
    {
        // Arrange
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;
        List<FundoDto> fundos = new List<FundoDto> { new FundoDto { Codigo = "TEST01", Nome = "Test Fund" } };
        _fundoServiceMock
            .Setup(s => s.GetAllAsync(token))
            .ReturnsAsync(fundos);

        // Act
        IActionResult result = await _controller.Get(token);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _fundoServiceMock.Verify(s => s.GetAllAsync(token), Times.Once);
    }

    /// <summary>
    /// Tests that Get works correctly when CancellationToken is in a cancelled state.
    /// </summary>
    [Fact]
    public async Task Get_WithCancelledToken_PropagatesCancellation()
    {
        // Arrange
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel();
        CancellationToken token = cts.Token;
        _fundoServiceMock
            .Setup(s => s.GetAllAsync(token))
            .ThrowsAsync(new TaskCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await _controller.Get(token));
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching all funds")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Get works correctly with default CancellationToken.
    /// </summary>
    [Fact]
    public async Task Get_WithDefaultCancellationToken_CallsServiceSuccessfully()
    {
        // Arrange
        List<FundoDto> fundos = new List<FundoDto>
        {
            new FundoDto
            {
                Codigo = _faker.Random.AlphaNumeric(10),
                Nome = _faker.Company.CompanyName(),
                Cnpj = _faker.Random.Replace("##.###.###/####-##"),
                CodigoTipo = _faker.Random.Int(1, 10),
                NomeTipo = _faker.Finance.AccountName(),
                Patrimonio = _faker.Finance.Amount()
            },
            new FundoDto
            {
                Codigo = _faker.Random.AlphaNumeric(10),
                Nome = _faker.Company.CompanyName(),
                Cnpj = _faker.Random.Replace("##.###.###/####-##"),
                CodigoTipo = _faker.Random.Int(1, 10),
                NomeTipo = _faker.Finance.AccountName(),
                Patrimonio = _faker.Finance.Amount()
            }
        };
        _fundoServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundos);

        // Act
        IActionResult result = await _controller.Get();

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<FundoDto> returnedFundos = Assert.IsAssignableFrom<IEnumerable<FundoDto>>(okResult.Value);
        Assert.Equal(2, returnedFundos.Count());
        _fundoServiceMock.Verify(s => s.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that Get propagates exceptions from the service.
    /// </summary>
    [Fact]
    public async Task Get_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        Exception expectedException = new InvalidOperationException("Database connection failed");
        _fundoServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _controller.Get());
        Assert.Equal("Database connection failed", exception.Message);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching all funds")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Get returns OkObjectResult with fund data when service returns a valid fund.
    /// </summary>
    [Fact]
    public async Task Get_WhenServiceReturnsValidFund_ReturnsOkResultWithFund()
    {
        // Arrange
        var faker = new Faker();
        var codigo = "ITAUTESTE01";
        var fundoDto = new FundoDto
        {
            Codigo = codigo,
            Nome = faker.Company.CompanyName(),
            Cnpj = faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = faker.Random.Int(1, 10),
            NomeTipo = faker.Finance.AccountName(),
            Patrimonio = faker.Finance.Amount()
        };
        var mockFundoService = new Mock<IFundoService>();
        mockFundoService.Setup(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundoDto);
        var mockLogger = new Mock<ILogger<FundoController>>();
        var controller = new FundoController(mockFundoService.Object, mockLogger.Object);

        // Act
        var result = await controller.Get(codigo, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedFundo = Assert.IsType<FundoDto>(okResult.Value);
        Assert.Equal(fundoDto.Codigo, returnedFundo.Codigo);
        Assert.Equal(fundoDto.Nome, returnedFundo.Nome);
        mockFundoService.Verify(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that Get returns NotFoundResult when service returns null.
    /// </summary>
    [Fact]
    public async Task Get_WhenServiceReturnsNull_ReturnsNotFoundResult()
    {
        // Arrange
        var codigo = "NONEXISTENT";
        var mockFundoService = new Mock<IFundoService>();
        mockFundoService.Setup(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FundoDto?)null);
        var mockLogger = new Mock<ILogger<FundoController>>();
        var controller = new FundoController(mockFundoService.Object, mockLogger.Object);

        // Act
        var result = await controller.Get(codigo, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        mockFundoService.Verify(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that Get logs information message when invoked with valid codigo.
    /// </summary>
    [Fact]
    public async Task Get_WhenInvoked_LogsInformationMessage()
    {
        // Arrange
        var codigo = "ITAUTESTE01";
        var fundoDto = new FundoDto { Codigo = codigo };
        var mockFundoService = new Mock<IFundoService>();
        mockFundoService.Setup(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundoDto);
        var mockLogger = new Mock<ILogger<FundoController>>();
        var controller = new FundoController(mockFundoService.Object, mockLogger.Object);

        // Act
        await controller.Get(codigo, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching fund with codigo")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Get logs warning message when service returns null.
    /// </summary>
    [Fact]
    public async Task Get_WhenServiceReturnsNull_LogsWarningMessage()
    {
        // Arrange
        var codigo = "NONEXISTENT";
        var mockFundoService = new Mock<IFundoService>();
        mockFundoService.Setup(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FundoDto?)null);
        var mockLogger = new Mock<ILogger<FundoController>>();
        var controller = new FundoController(mockFundoService.Object, mockLogger.Object);

        // Act
        await controller.Get(codigo, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Get does not log warning message when service returns a valid fund.
    /// </summary>
    [Fact]
    public async Task Get_WhenServiceReturnsValidFund_DoesNotLogWarning()
    {
        // Arrange
        var codigo = "ITAUTESTE01";
        var fundoDto = new FundoDto { Codigo = codigo };
        var mockFundoService = new Mock<IFundoService>();
        mockFundoService.Setup(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundoDto);
        var mockLogger = new Mock<ILogger<FundoController>>();
        var controller = new FundoController(mockFundoService.Object, mockLogger.Object);

        // Act
        await controller.Get(codigo, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that Get passes cancellation token to the service.
    /// </summary>
    [Fact]
    public async Task Get_WhenCancellationTokenProvided_PassesTokenToService()
    {
        // Arrange
        var codigo = "ITAUTESTE01";
        var fundoDto = new FundoDto { Codigo = codigo };
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var mockFundoService = new Mock<IFundoService>();
        mockFundoService.Setup(s => s.GetByCodigoAsync(codigo, cancellationToken))
            .ReturnsAsync(fundoDto);
        var mockLogger = new Mock<ILogger<FundoController>>();
        var controller = new FundoController(mockFundoService.Object, mockLogger.Object);

        // Act
        await controller.Get(codigo, cancellationToken);

        // Assert
        mockFundoService.Verify(s => s.GetByCodigoAsync(codigo, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Get handles empty string codigo appropriately.
    /// </summary>
    [Fact]
    public async Task Get_WhenCodigoIsEmpty_CallsServiceAndReturnsResult()
    {
        // Arrange
        var codigo = string.Empty;
        var mockFundoService = new Mock<IFundoService>();
        mockFundoService.Setup(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FundoDto?)null);
        var mockLogger = new Mock<ILogger<FundoController>>();
        var controller = new FundoController(mockFundoService.Object, mockLogger.Object);

        // Act
        var result = await controller.Get(codigo, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        mockFundoService.Verify(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that Get handles whitespace-only codigo appropriately.
    /// </summary>
    [Fact]
    public async Task Get_WhenCodigoIsWhitespace_CallsServiceAndReturnsResult()
    {
        // Arrange
        var codigo = "   ";
        var mockFundoService = new Mock<IFundoService>();
        mockFundoService.Setup(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FundoDto?)null);
        var mockLogger = new Mock<ILogger<FundoController>>();
        var controller = new FundoController(mockFundoService.Object, mockLogger.Object);

        // Act
        var result = await controller.Get(codigo, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        mockFundoService.Verify(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that Get handles codigo with special characters appropriately.
    /// </summary>
    [Theory]
    [InlineData("FUND@2024")]
    [InlineData("FUND#123")]
    [InlineData("FUND$TEST")]
    [InlineData("FUND%001")]
    [InlineData("FUND&CO")]
    public async Task Get_WhenCodigoHasSpecialCharacters_CallsServiceAndReturnsResult(string codigo)
    {
        // Arrange
        var fundoDto = new FundoDto { Codigo = codigo };
        var mockFundoService = new Mock<IFundoService>();
        mockFundoService.Setup(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundoDto);
        var mockLogger = new Mock<ILogger<FundoController>>();
        var controller = new FundoController(mockFundoService.Object, mockLogger.Object);

        // Act
        var result = await controller.Get(codigo, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedFundo = Assert.IsType<FundoDto>(okResult.Value);
        Assert.Equal(codigo, returnedFundo.Codigo);
    }

    /// <summary>
    /// Tests that Get handles very long codigo string appropriately.
    /// </summary>
    [Fact]
    public async Task Get_WhenCodigoIsVeryLong_CallsServiceAndReturnsResult()
    {
        // Arrange
        var codigo = new string('A', 1000);
        var fundoDto = new FundoDto { Codigo = codigo };
        var mockFundoService = new Mock<IFundoService>();
        mockFundoService.Setup(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundoDto);
        var mockLogger = new Mock<ILogger<FundoController>>();
        var controller = new FundoController(mockFundoService.Object, mockLogger.Object);

        // Act
        var result = await controller.Get(codigo, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedFundo = Assert.IsType<FundoDto>(okResult.Value);
        Assert.Equal(codigo, returnedFundo.Codigo);
    }

    /// <summary>
    /// Tests that Get handles codigo with control characters appropriately.
    /// </summary>
    [Fact]
    public async Task Get_WhenCodigoHasControlCharacters_CallsServiceAndReturnsResult()
    {
        // Arrange
        var codigo = "FUND\t\n\r123";
        var mockFundoService = new Mock<IFundoService>();
        mockFundoService.Setup(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FundoDto?)null);
        var mockLogger = new Mock<ILogger<FundoController>>();
        var controller = new FundoController(mockFundoService.Object, mockLogger.Object);

        // Act
        var result = await controller.Get(codigo, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        mockFundoService.Verify(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that Get handles codigo with unicode characters appropriately.
    /// </summary>
    [Fact]
    public async Task Get_WhenCodigoHasUnicodeCharacters_CallsServiceAndReturnsResult()
    {
        // Arrange
        var codigo = "FUND测试01";
        var fundoDto = new FundoDto { Codigo = codigo };
        var mockFundoService = new Mock<IFundoService>();
        mockFundoService.Setup(s => s.GetByCodigoAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundoDto);
        var mockLogger = new Mock<ILogger<FundoController>>();
        var controller = new FundoController(mockFundoService.Object, mockLogger.Object);

        // Act
        var result = await controller.Get(codigo, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedFundo = Assert.IsType<FundoDto>(okResult.Value);
        Assert.Equal(codigo, returnedFundo.Codigo);
    }

    /// <summary>
    /// Tests that Put returns Ok when the fund is successfully updated.
    /// Condition: Fund exists and UpdateAsync returns true.
    /// Expected: Returns OkResult and logs success messages.
    /// </summary>
    [Fact]
    public async Task Put_WhenFundExistsAndUpdatedSuccessfully_ReturnsOk()
    {
        // Arrange
        var codigo = _faker.Random.AlphaNumeric(10);
        var dto = new UpdateFundoDto
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10)
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateAsync(codigo, dto, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Put(codigo, dto, cancellationToken);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateAsync(codigo, dto, cancellationToken), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Updating fund with codigo '{codigo}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fund with codigo '{codigo}' updated successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Put returns NotFound when the fund does not exist.
    /// Condition: UpdateAsync returns false indicating fund not found.
    /// Expected: Returns NotFoundResult and logs warning.
    /// </summary>
    [Fact]
    public async Task Put_WhenFundDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var codigo = _faker.Random.AlphaNumeric(10);
        var dto = new UpdateFundoDto
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10)
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateAsync(codigo, dto, cancellationToken))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Put(codigo, dto, cancellationToken);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateAsync(codigo, dto, cancellationToken), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Updating fund with codigo '{codigo}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fund with codigo '{codigo}' not found for update")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Put handles empty string codigo parameter.
    /// Condition: codigo is an empty string.
    /// Expected: Calls UpdateAsync with empty string and processes normally.
    /// </summary>
    [Fact]
    public async Task Put_WhenCodigoIsEmpty_ProcessesNormally()
    {
        // Arrange
        var codigo = string.Empty;
        var dto = new UpdateFundoDto
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10)
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateAsync(codigo, dto, cancellationToken))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Put(codigo, dto, cancellationToken);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateAsync(codigo, dto, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Put handles whitespace-only codigo parameter.
    /// Condition: codigo contains only whitespace characters.
    /// Expected: Calls UpdateAsync with whitespace string and processes normally.
    /// </summary>
    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData(" \t\n ")]
    public async Task Put_WhenCodigoIsWhitespace_ProcessesNormally(string codigo)
    {
        // Arrange
        var dto = new UpdateFundoDto
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10)
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateAsync(codigo, dto, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Put(codigo, dto, cancellationToken);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateAsync(codigo, dto, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Put handles codigo with special characters.
    /// Condition: codigo contains special characters.
    /// Expected: Processes the codigo normally with special characters.
    /// </summary>
    [Theory]
    [InlineData("CÓDIGO@123")]
    [InlineData("TEST#$%")]
    [InlineData("ABC-123_XYZ")]
    [InlineData("ФонДЪ123")]
    [InlineData("基金123")]
    public async Task Put_WhenCodigoContainsSpecialCharacters_ProcessesNormally(string codigo)
    {
        // Arrange
        var dto = new UpdateFundoDto
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10)
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateAsync(codigo, dto, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Put(codigo, dto, cancellationToken);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateAsync(codigo, dto, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Put handles very long codigo strings.
    /// Condition: codigo is an extremely long string.
    /// Expected: Processes the long codigo normally without errors.
    /// </summary>
    [Fact]
    public async Task Put_WhenCodigoIsVeryLong_ProcessesNormally()
    {
        // Arrange
        var codigo = new string('A', 10000);
        var dto = new UpdateFundoDto
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10)
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateAsync(codigo, dto, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Put(codigo, dto, cancellationToken);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateAsync(codigo, dto, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Put properly passes cancellation token to the service.
    /// Condition: A specific cancellation token is provided.
    /// Expected: The token is passed to UpdateAsync correctly.
    /// </summary>
    [Fact]
    public async Task Put_WithCancellationToken_PassesTokenToService()
    {
        // Arrange
        var codigo = _faker.Random.AlphaNumeric(10);
        var dto = new UpdateFundoDto
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10)
        };
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        _fundoServiceMock
            .Setup(s => s.UpdateAsync(codigo, dto, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Put(codigo, dto, cancellationToken);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateAsync(codigo, dto, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Put handles when cancellation token is already cancelled.
    /// Condition: Cancellation token is cancelled before the call.
    /// Expected: OperationCanceledException is thrown if service respects cancellation.
    /// </summary>
    [Fact]
    public async Task Put_WhenCancellationTokenIsCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var codigo = _faker.Random.AlphaNumeric(10);
        var dto = new UpdateFundoDto
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10)
        };
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancellationToken = cts.Token;
        _fundoServiceMock
            .Setup(s => s.UpdateAsync(codigo, dto, cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _controller.Put(codigo, dto, cancellationToken));
    }

    /// <summary>
    /// Tests that Put handles exceptions thrown by UpdateAsync.
    /// Condition: UpdateAsync throws an exception.
    /// Expected: Exception propagates to caller.
    /// </summary>
    [Fact]
    public async Task Put_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        var codigo = _faker.Random.AlphaNumeric(10);
        var dto = new UpdateFundoDto
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10)
        };
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Test exception");
        _fundoServiceMock
            .Setup(s => s.UpdateAsync(codigo, dto, cancellationToken))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _controller.Put(codigo, dto, cancellationToken));
        Assert.Equal("Test exception", exception.Message);
    }

    /// <summary>
    /// Tests that Put uses default cancellation token when not provided.
    /// Condition: cancellationToken parameter is omitted (defaults to CancellationToken.None).
    /// Expected: Service is called with default cancellation token.
    /// </summary>
    [Fact]
    public async Task Put_WithoutCancellationToken_UsesDefaultToken()
    {
        // Arrange
        var codigo = _faker.Random.AlphaNumeric(10);
        var dto = new UpdateFundoDto
        {
            Nome = _faker.Company.CompanyName(),
            Cnpj = _faker.Random.Replace("##.###.###/####-##"),
            CodigoTipo = _faker.Random.Int(1, 10)
        };
        _fundoServiceMock
            .Setup(s => s.UpdateAsync(codigo, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Put(codigo, dto);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(
            s => s.UpdateAsync(codigo, dto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Put correctly processes different UpdateFundoDto configurations.
    /// Condition: Various valid DTO configurations.
    /// Expected: All valid DTOs are processed successfully.
    /// </summary>
    [Theory]
    [InlineData("Fund A", "12.345.678/0001-90", 1)]
    [InlineData("Fund B", "98.765.432/0001-10", 999)]
    [InlineData("X", "00.000.000/0000-00", 0)]
    [InlineData("Very Long Fund Name That Exceeds Normal Length Expectations", "11.111.111/1111-11", int.MaxValue)]
    public async Task Put_WithVariousDtoConfigurations_ProcessesSuccessfully(string nome, string cnpj, int codigoTipo)
    {
        // Arrange
        var codigo = _faker.Random.AlphaNumeric(10);
        var dto = new UpdateFundoDto
        {
            Nome = nome,
            Cnpj = cnpj,
            CodigoTipo = codigoTipo
        };
        var cancellationToken = CancellationToken.None;
        _fundoServiceMock
            .Setup(s => s.UpdateAsync(codigo, dto, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Put(codigo, dto, cancellationToken);

        // Assert
        Assert.IsType<OkResult>(result);
        _fundoServiceMock.Verify(s => s.UpdateAsync(codigo, dto, cancellationToken), Times.Once);
    }
}