
    $(function () {
        $('#date').datepick();
    });

    function JavascriptFunction() {
        var url = '@Url.Action("PostMethod", "Branch")';
        $("#divLoading").show();
        $.post(url, null,
                function (data) {
                    $("#PID")[0].innerHTML = data;
                    $("#divLoading").hide();
                });
    }


    function fetchtotal(qty_id) {

        //debugger;
        var grandtotal = 0;
        var quantity = $('#' + qty_id).val();
        var arr = [];
        arr = qty_id.split('-')
        var aa = arr[1];

        var deno = $('#deno' + aa).val();
        var total = deno * quantity;

        $('#t-' + aa).val(total.toFixed(0));

        for (var i = 1; i <= 10; i++) {

            grandtotal += parseFloat($('#t-' + i).val());
        }

        $('#total').val(grandtotal.toFixed(0));
        var balanceamount = $('#bal').val();

        if (grandtotal == balanceamount) {
            $(':input[type="submit"]').prop('disabled', false);
        }
        else {
            $(':input[type="submit"]').prop('disabled', true);
        }

    }


    function business() {
        debugger;
        var values = "";
        values = $("#date").val();
        var bdate = values;

        //var url = '@Url.Action("closings", "Operator")';
        var url = "../Operator/closings";
        $.get(url, { bdate: bdate }, function data(d) {
            debugger;
            $("#bal").val(d);


        });
    }
    function closetotal() {
        debugger;
        var values = "";
        values = $("#date").val();
        var bdate = values;
        var url = "../Operator/DailyClose";
        $.get(url, { bdate: bdate }, function data(d) {
            debugger;
            $("#RD").val(d[0].Amt_Deposit);
            $("#FD").val(d[0].fd);
            $("#MIS").val(d[0].mrsamount);
            $("#DD").val(d[0].dd);
            $("#Voucher").val(d[0].Voucher);
            $("#Saving").val(d[0].Salary);
            $("#debit").val(d[0].Rebate);
            $("#Otherfee").val(d[0].revivalfee);
            $("#totalrecieve").val(d[0].totalrecieve);
            $("#MISReturn").val(d[0].Late_Fee);
            $("#totalreturn").val(d[0].totalreturn);
            $("#Total").val(d[0].total);
        });
    }

    $(function () {
        $("#dialog1").dialog({
            modal: true,
            autoOpen: false,
            title: "jQuery Dialog",
            width: 500,
            height: 300,
            
        });
        $("#dialog2").dialog({
            modal: true,
            autoOpen: false,
            title: "jQuery Dialog",
            width: 500,
            height: 300,

        });
        $("#dialog3").dialog({
            modal: true,
            autoOpen: false,
            title: "jQuery Dialog",
            width: 500,
            height: 300,

        });
        $("#dialog4").dialog({
            modal: true,
            autoOpen: false,
            title: "jQuery Dialog",
            width: 500,
            height: 300,

        });
        $("#dialog5").dialog({
            modal: true,
            autoOpen: false,
            title: "jQuery Dialog",
            width: 500,
            height: 300,

        });
        $("#dialog6").dialog({
            modal: true,
            autoOpen: false,
            title: "jQuery Dialog",
            width: 500,
            height: 300,

        });
        $("#dialog7").dialog({
            modal: true,
            autoOpen: false,
            title: "jQuery Dialog",
            width: 500,
            height: 300,

        });
        $("#btnShow1").click(function () {
            debugger;
            $('#dialog1').dialog('open');
        });
        $("#btnShow2").click(function () {
            debugger;
            $('#dialog2').dialog('open');
        });
        $("#btnShow3").click(function () {
            debugger;
            $('#dialog3').dialog('open');
        });
        $("#btnShow4").click(function () {
            debugger;
            $('#dialog4').dialog('open');
        });
        $("#btnShow5").click(function () {
            debugger;
            $('#dialog5').dialog('open');
        });
        $("#btnShow6").click(function () {
            debugger;
            $('#dialog6').dialog('open');
        });
        $("#btnShow7").click(function () {
            debugger;
            $('#dialog7').dialog('open');
        });
    });
