using UnityEngine;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using Sych.ShareAssets.Runtime;
using System.Collections.Generic;
using System;
using NPOI.Util;

/// <summary>
/// Classe responsável por formatar dados a uma planilha.
/// </summary>
public class SheetFormater
{
    private string defaultSheetName = "Patrimônios";
    private static string fileName = "Itens.xlsx";
    public string path = Path.Combine(Application.temporaryCachePath, fileName);

    /// <summary>
    /// Formato de retorno da função <seealso cref="SheetFormater.Format(int, string, string)"/>
    /// </summary>
    public class FormatResult
    {
        /// <summary>
        /// Indica se a operação concluída com sucesso (<c>true</c>) ou não (<c>false</c>).
        /// </summary>
        public bool Sucess { get; set; }

        /// <summary>
        /// Mensagem que descreve o retorno, seja erro e explicando ele, ou sucesso.
        /// </summary>
        /// <example>
        /// <code>
        /// formatResult.Response = "OK"; // Sucesso.
        /// 
        /// formatResult.Response = "Não foi possível criar por tal"; // Falha.
        /// </code>
        /// </example>
        public string Response { get; set; }

        /// <summary>
        /// Caminho que a planilha será criada.
        /// </summary>

        public string FilePath { get; set; }
    }

    /// <summary>
    /// Adiciona um registro (ID, Nome) em uma planilha Excel (.xlsx).
    /// </summary>
    /// <param name="ID">Número de 6 dígitos ou mais que identifica o item.</param>
    /// <param name="Name">Nome do item a ser registrado.</param>
    /// <param name="SheetName">Nome da planilha (opcional, default: "Itens").</param>
    /// <returns>Objeto <see cref="FormatResult"/> indicando sucesso ou falha da operação.</returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando <paramref name="ID"/> tiver menos de 6 dígitos
    /// ou quando <paramref name="Name"/> for nulo ou vazio.
    /// </exception>
    public FormatResult Format(int ID, string Name, string SheetName = "")
    {
        if (ID.ToString().Length < 6)
            return new FormatResult { Sucess = false, Response = "O ID precisa ter pelo menos 6 dígitos." };

        if (string.IsNullOrWhiteSpace(Name))
            return new FormatResult { Sucess = false, Response = "O Nome não pode estar vazio." };

        if (string.IsNullOrEmpty(SheetName))
            SheetName = defaultSheetName;

        if (!File.Exists(path))
        {
            path = Path.Combine(Application.temporaryCachePath, fileName);
        }

        IWorkbook workbook = SheetCreate();
        ISheet sheet;

        sheet = workbook.GetSheet(SheetName) ?? workbook.CreateSheet(SheetName);

        if (sheet.PhysicalNumberOfRows == 0)
        {
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("ID");
            header.CreateCell(1).SetCellValue("Nome");
        }

        for (int i = 1; i < sheet.PhysicalNumberOfRows; i++)
        {
            var row = sheet.GetRow(i);
            if (row != null && row.GetCell(0) != null && row.GetCell(0).ToString() == ID.ToString())
                return new FormatResult { Sucess = false, Response = "Item já existe." };
        }

        int newRowNum = sheet.PhysicalNumberOfRows;
        var newRow = sheet.CreateRow(newRowNum);
        newRow.CreateCell(0).SetCellValue(ID);
        newRow.CreateCell(1).SetCellValue(Name);

        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            workbook.Write(fs);

        return new FormatResult { Sucess = true, Response = $"Item {Name} adicionado com sucesso.", FilePath = path };
    }

    /// <summary>
    /// Adiciona um registro (ID, Nome) em uma planilha Excel usando o nome padrão "Itens".
    /// </summary>
    /// <param name="ID">Número de 6 dígitos ou mais que identifica o item.</param>
    /// <param name="Name">Nome do item a ser registrado.</param>
    /// <returns>Objeto <see cref="FormatResult"/> indicando sucesso ou falha da operação.</returns>
    public FormatResult Format(int ID, string Name)
    {
        return Format(ID, Name, "Itens");
    }

    private XSSFWorkbook SheetCreate()
    {
        if (File.Exists(path))
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                return new XSSFWorkbook(fs);
        else
            return new XSSFWorkbook();
    }

    /// <summary>
    /// Compartilha a planilha com o usuário via Android ou IOs
    /// </summary>
    /// <param name="path">Caminho para o arquivo da planilha.</param>
    public async void SheetShare(string path)
    {
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            try
            {
                foreach (var item in new List<string> { path })
                {
                    bool success = await Share.ItemAsync(item);
                    Debug.Log($"Compartilhamento: {(success ? "sucesso" : "falha")}");

                    if (success)
                    {
                        Debug.Log("Planilha compartilhada e removida do cache.");
                        File.Delete(path);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erro ao compartilhar a planilha: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Caminho para planilha é nulo.");
        }
    }
}