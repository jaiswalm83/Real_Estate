    $(document).ready(function () {
        $("#BranchCode").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: "/Admin/AutoCompleteBrachcode",
                    type: "POST",
                    dataType: "json",
                    data: { term: request.term },
                    success: function (data) {
                        response($.map(data, function (item) {
                            return { label: item.BranchName, value: item.BranchCode };
                        }))

                    }
                })
            },
            messages: {
                noResults: "", results: ""
            }
        });
    })

    $(document).ready(function () {
        $("#newagentid").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: "/Admin/AutoCompleteAgentid",
                    type: "POST",
                    dataType: "json",
                    data: { term: request.term },
                    success: function (data) {
                        response($.map(data, function (item) {
                            return { label: item.name, value: item.NewAgentId };
                        }))

                    }
                })
            },
            messages: {
                noResults: "", results: ""
            }
        });
    })
