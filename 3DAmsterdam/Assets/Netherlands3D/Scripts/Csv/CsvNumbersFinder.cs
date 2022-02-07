﻿using Netherlands3D.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class CsvNumbersFinder : CsvContentFinder
{
    public string IDColumnName = "";

    public int IDColumnIndex = 0;
    public int NumberColumnIndex = 0;

    public List<int> IDColumnIndices = new List<int>();
    public List<int> NumberColumnIndices = new List<int>();

    public CsvNumbersFinder(string[] Columns, List<string[]> Rows)
    {
        this.Columns = Columns;
        this.Rows = Rows;

        GetNumberColumns();

        if (Columns.Length == 1)
        {
            Status = CsvContentFinderStatus.Failed;
            StatusMessageLines.Add("Geen kolommen gedetecteerd, controleer of de kolommen gescheiden zijn met het ; teken in het CSV bestand");
            return;
        }
        if (NumberColumnIndices.Count < 2)
        {
            Status = CsvContentFinderStatus.Failed;
            StatusMessageLines.Add("Geen kolommen met nummers/getallen gevonden.");
            return;
        }
    }

    void GetNumberColumns()
    {
        //finding the coordinate columns
        var list = new List<string>();
        var row1 = Rows.First();

        for (int i = 0; i < Columns.Length; i++)
        {
            var col = row1[i];
            double number = 0;
            if (ParseNumber(col, out number))
            {
                IDColumnIndices.Add(i); //Number columns can also be ID columns
                NumberColumnIndices.Add(i);
            }
            else
            {
                IDColumnIndices.Add(i);
                if (IDColumnName == "")
                {
                    IDColumnName = col;
                    IDColumnIndex = i;
                }
            }
        }

        Status = (NumberColumnIndices.Count > 0 && IDColumnIndices.Count > 0) ? CsvContentFinderStatus.Success : CsvContentFinderStatus.Failed;
    }

    public void SetIDColumn(string columnName)
    {
        for (int i = 0; i < Columns.Length; i++)
        {
            if (columnName == Columns[i])
            {
                IDColumnIndex = i;
            }
        }
    }

    public void SetNumberColumn(string columnName)
    {
        for (int i = 0; i < Columns.Length; i++)
        {
            if (columnName == Columns[i])
            {
                NumberColumnIndex = i;
            }
        }
    }

    public bool ParseNumber(string numberString, out double number)
    {
        return double.TryParse(numberString.Replace(",", "."), System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out number);
    }

    public Dictionary<string, float> GetNumbersAndIDs()
    {
        var idsAndNumbers = new Dictionary<string, float>();

        for (int i = 1; i < Rows.Count; i++)
        {
            var cols = Rows[i];
            var id = cols[IDColumnIndex];
            var numberText = cols[NumberColumnIndex];

            double outputNumber = 0;
            ParseNumber(numberText, out outputNumber);
            idsAndNumbers.Add(id, (float)outputNumber);
        }

        return idsAndNumbers;
    }
}

