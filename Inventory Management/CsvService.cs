public class CsvService
{
    public static async Task<DataTable> ConvertToDataTable(MultipartMemoryStreamProvider provider)
    {
        DataTable csvData = new DataTable();

        foreach (var file in provider.Contents)
        {
            using (TextFieldParser csvReader = new TextFieldParser(await file.ReadAsStreamAsync()))
            {
                csvReader.SetDelimiters(new string[] { "," });
                csvReader.HasFieldsEnclosedInQuotes = true;
                string[] colFields = csvReader.ReadFields();
                foreach (string column in colFields)
                {
                    DataColumn datecolumn = new DataColumn(column);
                    datecolumn.AllowDBNull = true;
                    csvData.Columns.Add(datecolumn);
                }
                while (!csvReader.EndOfData)
                {
                    string[] fieldData = csvReader.ReadFields();
                    //Making empty value as null
                    for (int i = 0; i < fieldData.Length; i++)
                    {
                        if (fieldData[i] == "")
                        {
                            fieldData[i] = null;
                        }
                    }
                    csvData.Rows.Add(fieldData);
                }
            }
        }

        return csvData;
    }
}