
//Table row class for tracking and functionality
var TableRow = function () { };
TableRow.prototype = {
	row: null,

	getRow: function () {
		return this.row;
	},
	setRow: function (newRow) {
		this.row = newRow;
	}
};

$(document).ready(function () {

	var tableRow = new TableRow();

	//Add row selection & highlighting to all table rows of class 'selection'
	$('table.selection tr:not(:has(th))').click(function (event) {
		if (tableRow.getRow() != null) {
			$(tableRow.getRow()).removeClass('rowSelected');
		}
		if (tableRow.getRow() == this)
		    return;

		$(this).addClass('rowSelected');
		tableRow.setRow(this);
	});

});
