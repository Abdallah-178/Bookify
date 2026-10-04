var updatedRow;
var isSubmitting = false;

var datatable, table; 

$.ajaxSetup({
    timeout: 15000
});


function showSuccessMessage() {
    Swal.fire({
        icon: "success",
        title: "Changed Successfuly",
        text: "Saved Successfully !",
        confirmButtonText: "Ok",
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}

function showErrorMessage(xhr) {
    var message = 'Something went wrong!';

    if (xhr?.statusText === 'timeout' || xhr?.status === 0) {
        message = 'Unable to connect to the server. Please check your internet connection and try again.';
    }

    Swal.fire({
        icon: 'error',
        title: 'Oops...',
        text: message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}


function disaablesubmitButton(btn) {
    $(btn).attr('disabled', 'disabled').attr('data-kt-indicator', 'on');

}

                // before request go to the Server
function onModalBegin() {

    if (isSubmitting) {
        return false; // يمنع الـ Ajax من الإرسال أصلاً
    }
    isSubmitting = true;
    disaablesubmitButton($('#Modal').find(':submit'));
}

function onModalSuccess(row) {
    showSuccessMessage();

    $('#Modal').modal('hide');

                // Create And Update Datatable 
    if (updatedRow !== undefined) {
        datatable.row(updatedRow).remove().draw();
        updatedRow = undefined;
    }
    var newRow = $(row);
    datatable.row.add(newRow).draw(); // When we use  DataTable Just

    //(Rerendering)Last Drop Down List When Use Just Mitronic
    KTMenu.init();
    KTMenu.initHandlers();
}


function onModalfailure(xhr) {
    showErrorMessage(xhr);

    if (xhr?.responseJSON?.errors) {
        $.each(xhr.responseJSON.errors, function (field, messages) {
            $('#Modal').find('[data-valmsg-for="' + field + '"]').text(messages[0]);
            $('#Modal').find('[name="' + field + '"]').addClass('input-validation-error');
        });
    }
}

function onModalComplete() {
    isSubmitting = false;
    $('body :submit').removeAttr('disabled').attr('data-kt-indicator', 'off');
}

// Select 2
function applaySelect2() {
    $('.js-select2').select2();
    $('.js-select2').on('select2:select', function (e) {
        var select = $(this);
        $('form').not('#SignOut').validate().element('#' + select.attr('id'));
    });
}

//Begin Handel DataTable

var KTDatatablesExample = function () {

    // Private functions
    var initDatatable = function () {


        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            "info": false,
            'pageLength': 10,
            'drawCallback': function () {
                KTMenu.createInstances();
            }
        });
    }

    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = $('.js-DatataTable').data('document-title');  // title for Document when you export
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: ':not(.js-no-export)'
                    }
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: ':not(.js-no-export)'
                    }
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: ':not(.js-no-export)'
                    }
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: ':not(.js-no-export)'
                    }
                }
            ]
        }).container().appendTo($('#kt_datatable_example_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var searchTimeout;

    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                datatable.search(e.target.value).draw();
            }, 1000);
        });
    }

    // Public methods
    return {
        init: function () {
            table = document.querySelector('.js-DatataTable'); // Datatable Name selecttor

            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            handleSearchDatatable();
        }
    };
}();

// End Handel Datatable


$(document).ready(function () {


    // Disable Submit Button
    $('form').not('#SignOut').on('submit', function () {

        if ($('.js-tinymce').length > 0) {
            $('.js-tinymce').each(function () {
                var input = $(this);
                var content = tinymce.get(input.attr('id')).getContent();
                input.val(content)
            });

        }
        var isvalid = $(this).valid();
        if (isvalid)
            disaablesubmitButton($(this).find(':submit'));
    });

    // Handel Description TinyMcE
    if ($('.js-tinymce').length > 0) {
        var options = {
            selector: ".js-tinymce",
            height: "480",
            menubar: false,
            toolbar: ["styleselect fontselect fontsizeselect",
                "undo redo | cut copy paste | bold italic | link image | alignleft aligncenter alignright alignjustify",
                "bullist numlist | outdent indent | blockquote subscript superscript | advlist | autolink | lists charmap | print preview |  code"],
            plugins: "advlist autolink link image lists charmap print preview code"
        };




        if (KTThemeMode.getMode() === "dark") {
            options["skin"] = "oxide-dark";
            options["content_css"] = "dark";
        }

        tinymce.init(options);

    }


    // Handel DatePicker
    $(".js-datepicker").daterangepicker({
        singleDatePicker: true,
        autoApply: true,
        drops: 'up',
        //  maxDate:new Date()

    });

    //Handel Select 2
    applaySelect2();

    // Handel Modal Page

    $('body').delegate('.js-render-modal', 'click', function () {  // Use delegate to get ajax setting

        var btn = $(this);
        var modal = $('#Modal');

        modal.find('.modal-title').text(btn.data('title')); // Render Title

        if (btn.data('update') !== undefined) { // For Update Row
            updatedRow = btn.parents('tr');
        }

        // Begin Ajax
        $.ajax({
            url: btn.data('url'),
            type: 'GET',

            success: function (form) {
                modal.find('.modal-body').html(form);
                $.validator.unobtrusive.parse(modal);  // after call Modal Apply Validation

                applaySelect2(); // IF Using Select 2

            },

            error: function (error) {
                showErrorMessage();
            }, complete: function () {
                btn.data('loading', false); // يرجع يسمح بالضغط تاني
            }
        });



        // End Ajax



        modal.modal('show');

    });
  
    // Handle Toggle Status
    $('body').delegate('.js-toggle-status', 'click', function () {

        var btn = $(this);
        if (btn.data('processing')) return;
        var message = $('#Message').text();

        bootbox.confirm({
            message: 'Are You Sure You Need To Toggle Status?',
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-success'
                }
            },
            callback: function (result) {

                if (result) {
                    btn.data('processing', true); // <== ناقص، لازم تضيفه
                    $.ajax({
                        url: btn.data('urltoggle'),
                        type: 'POST',
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (LastUpdatedOn) {

                            var row = btn.closest('tr');
                            var status = row.find('.js-status');

                            var isDeleted = status.text().trim() === 'Deleted';

                            if (isDeleted) {
                                status
                                    .text('Available')
                                    .removeClass('badge-light-danger')
                                    .addClass('badge-light-success');

                            } else {
                                status
                                    .text('Deleted')
                                    .removeClass('badge-light-success')
                                    .addClass('badge-light-danger');


                            }

                            row.find('.js-updated-on').html(LastUpdatedOn);
                            showSuccessMessage();
                        },
                        error: function () {
                            showErrorMessage();
                        },
                        complete: function () {
                            btn.data('processing', false);
                        }
                    });

                }
            }
        });

    });


    // Handel DataTable
    KTUtil.onDOMContentLoaded(function () {
        KTDatatablesExample.init();
    });


    // Handel Sign Out Button 
    $('.js-signout').on('click', function () {
        $('#SignOut').submit();
    });

    // Handel Confirm

});