var portfolioManager = function() {

    // appends a row to the portfolio items table.
    // @parentSelector: selector to append a row to.
    // @obj: portfolio item object to append.
    var appendRow = function (parentSelector, obj, price) {
        var tr = $("<tr data-id='" + obj.ItemId + "'></tr>");
        tr.append("<td class='name' > <a href='javascript:void(0)'  onclick='loadChart(\"" + obj.Symbol + "\")'>" + obj.Symbol + "</a></td>");
        tr.append("<td class='name' >" + obj.SharesNumber + "</td>");
        tr.append("<td class='name' >" + price + "</td>");
        tr.append("<td class='name' >" + price * obj.SharesNumber + "</td>");
        tr.append("<td><input type='button' class='update-button btn btn-info' value='Update' /><input type='button' class='delete-button btn btn-primary' value='Delete' /></td>");
        $(parentSelector).append(tr);
    }

    // adds all portfolio items as rows (deletes all rows before).
    // @parentSelector: selector to append a row to.
    // @tasks: array of portfolio items to append.
    var displayPortfolioItems = function (parentSelector, portfolioItems) {
        $(parentSelector).empty();

        var symbols = '';

        for (var i = 0; i < portfolioItems.length; i++) {
            symbols += portfolioItems[i].Symbol + '+';
        }
        symbols = symbols.slice(0, -1)
        var prices = '';
        $.ajax(
            {
                async: false,
                url: "/api/PortfolioItems/GetStocks",
                contentType: 'application/json',
                data: { 'symbols': symbols },
                success: function (response) {
                    prices = response;
                }
            });

        $.each(portfolioItems, function (i, item) {

            appendRow(parentSelector, item, prices[i]);
        });
 
    };

    // starts loading portfolio items from server.
    // @returns a promise.
    var loadPortfolioItems = function() {
        return $.getJSON("/api/PortfolioItems/GetLocal");
    };

    // starts creating a portfolio item on the server.
    // @symbol: symbol name.
    // @sharesNumber: number of shares.
    // @return a promise.
    var createPortfolio = function(symbol, sharesNumber) {
        return $.post("/api/portfolioitems",
        {
            Symbol: symbol,
            SharesNumber: sharesNumber
        });
    };

    // starts updating a portfolio item on the server.
    // @id: id of the portfolio item to update.
    // @symbol: symbol name.
    // @sharesNumber: number of shares.
    // @return a promise.
    var updatePortfolioItem = function(id, symbol, sharesNumber) {
        return $.ajax(
        {
            url: "/api/portfolioitems",
            type: "PUT",
            contentType: 'application/json',
            data: JSON.stringify({
                ItemId: id,
                Symbol: symbol,
                SharesNumber: sharesNumber
            })
        });
    };

    // starts deleting a portfolio item on the server.
    // @itemId: id of the item to delete.
    // @return a promise.
    var deletePortfolioItem = function (itemId) {
        return $.ajax({
            url: "/api/portfolioitems/" + itemId,
            type: 'DELETE'
        });
    };

    // returns public interface of portfolio manager.
    return {
        loadItems: loadPortfolioItems,
        displayItems: displayPortfolioItems,
        displayItem: appendRow,
        createItem: createPortfolio,
        deleteItem: deletePortfolioItem,
        updateItem: updatePortfolioItem
    };
}();
var operationManager = function () {

    // appends a row to the portfolio items table.
    // @parentSelector: selector to append a row to.
    // @obj: portfolio item object to append.
    var appendOpearationRow = function (parentSelector, obj) {
        var tr = $("<tr data-id='" + obj.ItemId + "'></tr>");
        tr.append("<td class='name' >" + obj.Operation + " " + obj.Symbol + " " + obj.SharesNumber+"</td>");        
        $(parentSelector).append(tr);

    };
    var displayOperations = function (parentSelector, portfolioItems) {
        $(parentSelector).empty();
        $.each(portfolioItems, function (i, item) {
            appendOpearationRow(parentSelector, item);
        });
    };
    var loadOperations = function () {
        return $.getJSON("/api/PortfolioItems/GetOperation");
    };
    return {
        loadItems: loadOperations,
        displayItems: displayOperations,
        displayItem: appendOpearationRow
    };
}();

function loadChart(symbol) {
    $('#chartContainer').empty();

    $.ajax(
{    async: false,
    url: "api/PortfolioItems/GetStockPrices",
    type: "Get",
    contentType: 'application/json',
    data: { 'symbol': symbol },//
    success: function (response) {
        alert(response == null)
        if (response == null)
            $('#chartContainer').append("<center><h1>Data Not Found</h1></center>")
        createChart(response);
    }
});
}

function createChart(items) {

    new Morris.Area({
        // ID of the element in which to draw the chart.
        element: 'chartContainer',
        // Chart data records -- each entry in this array corresponds to a point on
        // the chart.
        data:items,
        // The name of the data record attribute that contains x-values.
        xkey: 'year',
        // A list of names of data record attributes that contain y-values.
        ykeys: ['value'],
        // Labels for the ykeys -- will be displayed when you hover over the
        // chart.
        labels: ['Value'],
        fillOpacity: 0.6,
        hideHover: 'auto',
        behaveLikeLine: true,
        resize: true,
        pointFillColors: ['#ffffff'],
        pointStrokeColors: ['black'],
        xLabels:'month'
    });
}

$(function () {
    
    // add new portfolio item button click handler
    $("#newCreate").click(function() {
        var symbol = $('#symbol')[0].value;
        var sharesNumber = $('#sharesNumber')[0].value;
        var item = { Operation: "create", SharesNumber: sharesNumber, Symbol: symbol };
        operationManager.displayItem("#operation > tbody", item);

        portfolioManager.createItem(symbol, sharesNumber)
            .then(operationManager.loadItems).done
            (function (items) {
                operationManager.displayItems("#operation > tbody", items);
                portfolioManager.loadItems().done(function (data)
                {    portfolioManager.displayItems("#items > tbody", data);
                });                   
            });
    });

    // bind update portfolio item checkbox click handler
    $("#items > tbody").on('click', '.update-button', function () {
        var tr = $(this).parent().parent();
        var itemId = tr.attr("data-id");
        var symbol = $('#symbol')[0].value;
        var sharesNumber = $('#sharesNumber')[0].value;

        var item = { Operation: "update", SharesNumber: sharesNumber, Symbol: symbol };
        operationManager.displayItem("#operation > tbody", item);
        //tr.children()[0].innerText = symbol;
        //tr.children()[1].innerText = sharesNumber;


        //var symbol = tr.find('.symbol').text();
        //var sharesNumber = tr.find('.sharesNumber').text();
        
        portfolioManager.updateItem(itemId, symbol, sharesNumber)
            .then(operationManager.loadItems).done
            (function (items) {
                operationManager.displayItems("#operation > tbody", items);
                portfolioManager.loadItems().done(function (data) {
                    portfolioManager.displayItems("#items > tbody", data);
                });
            });

    });

    // bind delete button click for future rows
    $('#items > tbody').on('click', '.delete-button', function () {
        var tr = $(this).parent().parent();
        var itemId = tr.attr("data-id");
        
         var item = { Operation: "delete", SharesNumber: tr.children()[1].innerText, Symbol: tr.children()[0].innerText};
        operationManager.displayItem("#operation > tbody", item);

        portfolioManager.deleteItem(itemId)
            .then(operationManager.loadItems).done
            (function (items) {
                operationManager.displayItems("#operation > tbody", items);
                portfolioManager.loadItems().done(function (data) {
                    portfolioManager.displayItems("#items > tbody", data);
                });
            });
    });

    // load all items on startup
    portfolioManager.loadItems()
        .done(function(items) {
            portfolioManager.displayItems("#items > tbody", items);

            $.getJSON("api/PortfolioItems/GetRemote", function () {
                portfolioManager.loadItems()
                    .done(function (items) {
                        portfolioManager.displayItems("#items > tbody", items);
                    });
            });
           
        });
});