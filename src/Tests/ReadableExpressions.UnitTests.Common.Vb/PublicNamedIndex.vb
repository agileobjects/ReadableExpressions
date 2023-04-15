Public Class PublicNamedIndex(Of T)

    Public ReadOnly Property Value(
        Optional indexOne As Integer = 1,
        Optional indexTwo As Integer? = Nothing) As T
        Get
            Return Nothing
        End Get
    End Property

End Class