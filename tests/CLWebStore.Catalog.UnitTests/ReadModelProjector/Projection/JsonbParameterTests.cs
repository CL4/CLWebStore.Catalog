using Moq;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace CLWebStore.Catalog.UnitTests.ReadModelProjector.Projection;

public class JsonbParameterTests
{
    [Fact]
    public void AddParameter_AppendsNpgsqlParameterWithJsonbTypeAndValue()
    {
        // Arrange
        var json = "{ \"foo\": \"bar\" }";
        var param = new CLWebStore.Catalog.ReadModelProjector.Projection.JsonbParameter(json);

        var mockCommand = new Mock<IDbCommand>();
        var mockParameters = new Mock<IDataParameterCollection>();

        object? captured = null;
        mockParameters.Setup(p => p.Add(It.IsAny<object?>())).Returns(0).Callback<object?>(o => captured = o);

        mockCommand.SetupGet(c => c.Parameters).Returns(mockParameters.Object);

        // Act
        param.AddParameter(mockCommand.Object, "@p_json");

        // Assert
        Assert.NotNull(captured);
        Assert.IsType<NpgsqlParameter>(captured);

        var npgParam = (NpgsqlParameter)captured!;
        Assert.Equal(NpgsqlDbType.Jsonb, npgParam.NpgsqlDbType);
        Assert.Equal("p_json", npgParam.ParameterName);
        Assert.Equal(json, npgParam.Value);
    }
}
