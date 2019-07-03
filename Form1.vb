Imports System.Data.SqlClient
Imports System.Text
Public Class Form1
    Dim df1 As New DataFunctions.DataFunctions
    Dim gf1 As New GlobalFunction1.GlobalFunction1
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim TxtControlFile As String = "D:\saralwork\configfile\configfile.txt"
        gf1.SetGlobalVariables(TxtControlFile)
        Dim arr1 As List(Of String) = Database_Script("1_srv_1.3_mdf_3")
        Dim txt As Encoding = Encoding.UTF8
        Dim str As String() = arr1.ToArray
        gf1.StringWriteAllLines("C:\Users\USER\Desktop\script.txt", str, txt)
    End Sub
    Public Function Database_Script(ByVal server_database_name As String) As List(Of String)
        ' the below code is done to get all the details of database in the form of data table
        Dim dt As DataTable
        dt = df1.SqlExecuteDataTable(server_database_name, "select * from sys.tables")
        Dim arr As New List(Of String)
        For l = 0 To dt.Rows.Count - 1
            Dim dt_str As String = Convert.ToString(dt.Rows(l).Item("name"))
            Dim dt1 As DataTable
            dt1 = df1.GetSchemaInformations(server_database_name, dt_str)
            Dim primary_key_element As String = df1.GetPrimaryKey(server_database_name, dt_str)
            Dim index_element As String = ""
            Dim character_arr(dt1.Rows.Count - 1) As String
            Dim is_null(dt1.Rows.Count - 1) As String
            Dim index_str As String = ""
            Dim index_str1 As String = ""
            Dim dt2 As DataTable
            Dim dt2_str As String = "EXEC sp_helpindex" & " " & dt_str
            dt2 = df1.SqlExecuteDataTable(server_database_name, dt2_str)
            Dim dt3 As New DataTable
            Dim str_data As String = ""
            dt3 = df1.GetDataFromSql(server_database_name, dt_str)
            Dim dt_data As String = ""
            arr.Add("CREATE TABLE" & "  [" & "dbo" & "]." & " [" & dt_str & "](")
            For i = 0 To dt1.Rows.Count - 1
                ' the below code is get all the details from data table and insert them into list of string
                If Convert.ToString(dt1.Rows(i)("IS_NULLABLE")) = "YES" Then
                    is_null(i) = "NULL"
                Else
                    is_null(i) = "NOT NULL"
                End If
                If Convert.ToString(dt1.Rows(i)("CHARACTER_MAXIMUM_LENGTH")) = "" Then
                    character_arr(i) = Convert.ToString(dt1.Rows(i)("CHARACTER_MAXIMUM_LENGTH"))
                Else
                    character_arr(i) = "(" & Convert.ToString(dt1.Rows(i)("CHARACTER_MAXIMUM_LENGTH")) & ")"
                End If
                arr.Add("[" & dt1.Rows(i)("COLUMN_NAME") & "] " & "[" & dt1.Rows(i)("DATA_TYPE") & "]" & character_arr(i) & is_null(i) & ",")
            Next
            If dt2.Rows.Count <> 0 Then
                index_element = Convert.ToString(dt2.Rows(0).Item("index_keys"))
            End If
            ' the below code is insert value in list if table has only primary key
            If primary_key_element <> "" And index_element = "" Then
                arr.Add("CONSTRAINT" & " [PK_" & dt_str & "]" & "PRIMARY" & "KEY" & "CLUSTERED")
                arr.Add("(")
                arr.Add("[" & primary_key_element & "]" & " ASC")
                arr.Add(")" & "WITH" & "(PAD_INDEX = OFF," & " STATISTICS_NORECOMPUTE = OFF," & " IGNORE_DUP_KEY = OFF," & "ALLOW_ROW_LOCKS = ON," & "ALLOW_PAGE_LOCKS = ON)" & "ON [PRIMARY]")
                arr.Add(")")
                ' the below code is insert value in list if table has only index element
            ElseIf index_element <> "" And primary_key_element = "" Then
                For i = 0 To dt2.Rows.Count - 1
                    index_element = Convert.ToString(dt2.Rows(i).Item("index_keys"))
                    If index_element = primary_key_element Then
                        Continue For
                    Else
                        If Convert.ToString(dt2.Rows(i).Item("index_description")).Contains("unique") Then
                            index_str = "UNIQUE"
                            index_str1 = "IGNORE_DUP_KEY = OFF,"
                        Else
                            index_str = ""
                            index_str1 = ""
                        End If
                        arr.Add(")" & "ON [PRIMARY]")
                        arr.Add("CREATE " & index_str & " NONCLUSTERED INDEX" & "[" & Convert.ToString(dt2.Rows(i).Item("index_name")) & "] " & "ON" & "[" & "bdo" & "]." & "[" & dt_str & "]")
                        arr.Add("(")
                        arr.Add("[" & index_element & "]" & " ASC")
                        arr.Add(")" & "WITH" & "(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, " & index_str1 & " DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]")
                    End If
                Next
                ' the below code is insert value in list if table has both index element and primary key 
            ElseIf primary_key_element <> "" And index_element <> "" Then
                arr.Add("CONSTRAINT" & " [PK_" & dt_str & "]" & "PRIMARY KEY CLUSTERED")
                arr.Add("(")
                arr.Add("[" & primary_key_element & "]" & " ASC")
                arr.Add(")" & "WITH" & "(PAD_INDEX = OFF," & " STATISTICS_NORECOMPUTE = OFF," & " IGNORE_DUP_KEY = OFF," & "ALLOW_ROW_LOCKS = ON," & "ALLOW_PAGE_LOCKS = ON)" & "ON [PRIMARY]")
                arr.Add(")")
                For i = 0 To dt2.Rows.Count - 1
                    index_element = Convert.ToString(dt2.Rows(i).Item("index_keys"))
                    If primary_key_element = index_element Then
                        Continue For
                    Else
                        If Convert.ToString(dt2.Rows(i).Item("index_description")).Contains("unique") Then
                            index_str = "UNIQUE"
                            index_str1 = "IGNORE_DUP_KEY = OFF,"
                        Else
                            index_str = ""
                            index_str1 = ""
                        End If
                        arr.Add("CREATE " & index_str & " NONCLUSTERED INDEX" & " [" & Convert.ToString(dt2.Rows(i).Item("index_name")) & "] " & "ON" & "[" & "dbo" & "]." & "[" & dt_str & "]")
                        arr.Add("(")
                        arr.Add("[" & index_element & "]" & " ASC")
                        arr.Add(")" & "WITH" & "(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, " & index_str1 & " DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]")
                    End If
                Next
            Else
                'the below code is insert value in list if table has not both elements 
                arr.Add(")")
            End If
            arr.Add("")
            ' the below code is get all the details related to the insertion 
            For j = 0 To dt3.Columns.Count - 1
                If j = dt3.Columns.Count - 1 Then
                    str_data = str_data & "[" & dt3.Columns(j).ColumnName & "]"
                Else
                    str_data = str_data & "[" & dt3.Columns(j).ColumnName & "],"
                End If
            Next
            For i = 0 To dt3.Rows.Count - 1
                dt_data = Nothing
                For j = 0 To dt3.Columns.Count - 1
                    If j = dt3.Columns.Count - 1 Then
                        dt_data = dt_data & "'" & Convert.ToString(dt3.Rows(i)(j)) & "'"
                    Else
                        dt_data = dt_data & "'" & Convert.ToString(dt3.Rows(i)(j)) & "'" & ","
                    End If
                Next
                ' the below code is insert value in list related to the insertion 
                arr.Add("INSERT" & "[" & "dbo" & "]." & "[" & dt_str & "]" & "(" & str_data & ")" & " VALUES" & "(" & dt_data & ")")
            Next
            arr.Add("")
        Next
        Return arr
    End Function
End Class