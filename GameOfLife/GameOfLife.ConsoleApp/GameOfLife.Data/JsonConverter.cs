using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameOfLife.Data
{
    public class JsonConverter : JsonConverter<CellStatus[,]>
    {
        public override CellStatus[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            CellStatus[][]? rows = JsonSerializer.Deserialize<CellStatus[][]>(ref reader, options);

            if (rows == null)
                return new CellStatus[0, 0];

            int rowCount = rows.Length;
            int columnCount = rowCount == 0 ? 0 : rows[0]?.Length ?? 0;
            CellStatus[,] grid = new CellStatus[rowCount, columnCount];

            for (int row = 0; row < rowCount; row++)
            {
                if (rows[row] == null || rows[row].Length != columnCount)
                    throw new JsonException();

                for (int column = 0; column < columnCount; column++)
                {
                    grid[row, column] = rows[row][column];
                }
            }

            return grid;
        }

        public override void Write(Utf8JsonWriter writer, CellStatus[,] value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            CellStatus[][] rows = new CellStatus[value.GetLength(0)][];

            for (int row = 0; row < value.GetLength(0); row++)
            {
                rows[row] = new CellStatus[value.GetLength(1)];
                for (int column = 0; column < value.GetLength(1); column++)
                {
                    rows[row][column] = value[row, column];
                }
            }

            JsonSerializer.Serialize(writer, rows, options);
        }
    }
}
