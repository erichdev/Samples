Public Sub ValidatePunchSequence()
    Dim qd As QueryDef
    Dim rsPunchSeq As Recordset
    Dim lngEmpID As Long
    Dim datWorkDay As Date
    Dim bDirection As Boolean

    'begin by clearing the last list of punch errors
    Set qd = CurrentDb.QueryDefs("qdel_punch_errors_clear")
    qd.Execute (dbSeeChanges)

    'setup the qry and rs to loop thru the punches
    Set qd = CurrentDb.QueryDefs("qsel_validate_PunchSequence")
    Set rsPunchSeq = qd.OpenRecordset
    
    'move to the first rec and start the loop of recs
    If rsPunchSeq.RecordCount > 0 Then rsPunchSeq.MoveFirst
    Do While Not rsPunchSeq.EOF
        
        'grab the empID and start a loop for this employee
        lngEmpID = rsPunchSeq.Fields("fkEmployeeID")
        Do While rsPunchSeq.Fields("fkEmployeeID") = lngEmpID
        
            'check to see if the first entry is a punch IN
            If rsPunchSeq.Fields("bIn") <> True Then
                'if not, alert the user w/ an error
                'CHANGE - rmv msgbox and use an error log instead
                AddError lngEmpID, rsPunchSeq.Fields("WorkDate"), "First Punch is an OUT"
'                MsgBox "First Punch is OUT - " & rsPunchSeq.Fields("WorkDate")
                MoveToNextDay rsPunchSeq, rsPunchSeq.Fields("WorkDate")
            
            Else
                'otherwise, grab the date and punch direction of this punch
                datWorkDay = rsPunchSeq.Fields("WorkDate")
                bDirection = rsPunchSeq.Fields("bIn")
                'then, move to the next entry
                
                If (rsPunchSeq.Fields("fkEmployeeID") = 3152) Then
                    Debug.Print "placeholder"
                End If
                    
                rsPunchSeq.MoveNext
                
                'a temp check to prevent errors when the end of rs is reached.
                If rsPunchSeq.EOF Then
                    AddError lngEmpID, datWorkDay, "Missing Punch at End of File (EOF)"
                    Exit Do
                End If
                
                'check to see if the next entry is the same emp and date
                Do While (lngEmpID = rsPunchSeq.Fields("fkEmployeeID")) And (datWorkDay = rsPunchSeq.Fields("WorkDate"))
                    
                    'next, check the direction of the punch to make sure it is toggled
                    If bDirection = rsPunchSeq.Fields("bIn") Then
                        'if it didn't change, alert the user to the error
                        'CHANGE - rmv msgbox and use an error log instead
                        AddError lngEmpID, rsPunchSeq.Fields("WorkDate"), "Punch is Missing or Out of Sequence"
'                        MsgBox "Punch is out of Sequence - " & rsPunchSeq.Fields("WorkDate")
                        MoveToNextDay rsPunchSeq, rsPunchSeq.Fields("WorkDate")
                        
                    Else
                        'otherwise, if OK, then save this punch direction (IN/OUT) and move
                        'to the next record
                        bDirection = rsPunchSeq.Fields("bIn")
                        rsPunchSeq.MoveNext
                        
                    End If
                    
                    'another check to prevent errors when the end of rs is reached.
                    If rsPunchSeq.EOF Then
                        'if it is EOF, alert the user of an error if the rs ended
                        ' on a punchIN...
                        If bDirection = True Then AddError lngEmpID, datWorkDay, "Last Punch Entry is an IN"
                        Exit Do
                    ElseIf lngEmpID <> rsPunchSeq.Fields("fkEmployeeID") Then
                        'if it isn't EOF, but is a new employee, still alert the user of an error
                        ' when the last punch is a punchIN...
                        If bDirection = True Then AddError lngEmpID, datWorkDay, "Last Punch Entry is an IN"
                        Exit Do
                    End If
                Loop
                
                'check to see if moved to new employee w/out finding the matching OUT record
                ' and post an error for 'last punch IN'...
                If rsPunchSeq.EOF Then
                    If bDirection = True Then AddError lngEmpID, datWorkDay, "Last Punch Entry is an IN"
                    Exit Do
                ElseIf lngEmpID <> rsPunchSeq.Fields("fkEmployeeID") And bDirection = True Then
                    AddError lngEmpID, datWorkDay, "Last Punch Entry is an IN"
                ElseIf lngEmpID = rsPunchSeq.Fields("fkEmployeeID") And _
                            bDirection = rsPunchSeq.Fields("bIn") And _
                            rsPunchSeq.Fields("bIn") = True Then
                    AddError lngEmpID, datWorkDay, "Last Punch Entry is an IN"
                End If

            End If
        
            'final check to see if this is the end of the rs to avoid errors
            If rsPunchSeq.EOF Then
                'no need to check or alert for errors just prevent reading if EOF
                Exit Do
            End If
        
        Loop
    Loop
    
End Sub


Private Sub AddError(lngID As Long, dat As Date, msg As String)
    Dim qd As QueryDef

    Set qd = CurrentDb.QueryDefs("qapd_punch_errors_insert")
    qd.Parameters("@EmpApptDistID") = lngID
    qd.Parameters("@WorkDate") = dat
    qd.Parameters("@Message") = msg
    qd.Execute
    
    Set qd = Nothing
    
End Sub


Private Sub MoveToNextDay(ByRef rs As Recordset, ByVal dat As Date)
    Do While (rs.Fields("WorkDate") = dat)
        rs.MoveNext
        If rs.EOF Then Exit Do
    Loop
End Sub