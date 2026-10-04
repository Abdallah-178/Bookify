$(document).ready(function () {

    // Search input from  (Metronic) you can use Your Own Search
    $('[data-kt-filter="search"]').on('keyup', function () {
        datatable.search(this.value).draw();
    });


    var datatable = $('#Books').DataTable({

        serverSide: true,
        processing: true,
        stateSave: true,
        lengthMenu: [5, 10, 15, 20],
        language: {
            processing: '<div class="d-flex justify-content-center text-primary align-items-center dt-spinner"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div><span class="text-muted ps-2">Loading...</span></div>'
        },

        ajax: {
            url: '/Books/GetBooks',
            type: 'POST',
            data: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() }
        },

        'drawCallback': function () { KTMenu.createInstances(); } // For Open Action DropDownList
        ,
        order: [[1, 'desc']],
        columnDefs: [{
            targets: [0],
            visible: false,
            searchable: false
        }],
        columns: [
            { data: "id", name: "Id", "className": "d-none" },
            {
                "name": "Title", "render": function (data, type, row) {
                    const imageUrl = row.imageUrl == null ? '/assets/images/Books/no_img.png' : `/assets/images/Books/thumb/${row.custom_img}`;

                    return `
                                    <div class="d-flex align-items-center">
                                        <div class="symbol symbol-50px overflow-hidden me-3">
                                            <a href="/Books/Details/${row.id}">
                                                <div class="symbol-label h-70px">
                                                    <img src="${imageUrl}" alt="cover Img" class="w-100">
                                                </div>
                                            </a>
                                        </div>
                                        <div class="d-flex flex-column">
                                            <a href="/Books/Details/${row.id}" class="text-primary fw-bolder mb-1">
                                                ${row.title}
                                            </a>
                                            <span>${row.author}</span>
                                        </div>
                                    </div>`;
                }

            }, //End Render Title and Img

            { data: "publisher", name: "publisher" },
            {
                name: "PublishingDate", "render": function (data, type, row) {
                    return moment(row.publishingDate) // library js for Date&Time
                        .format('ll');

                }
            },// End PublishingDate
            { data: "hall", name: "hall" },
            { data: "categories", name: "categories", "orderable": false },
            {
                name: "IsAvilableForRentel", "render": function (data, type, row) {

                    return `<span class="badge badge-light-${(row.isAvilableForRentel ? "success" : "warning")} ">
                          ${(row.isAvilableForRentel ? "Available" : "Not Available")}
                             </span>`
                }
            },
            {
                name: "isDeleted", "render": function (data, type, row) {
                    return `<span class="badge badge-light-${(row.isDeleted ? "danger" : "success")} js-status">
                          ${(row.isDeleted ? "Deleted" : "Available")}
                             </span>`
                }
            },
            // Action Buttons
            {
                ordarable: false, "render": function (data, type, row) {

                    return `<div class="text-end">

            <a href="#" class="btn btn-light btn-active-light-primary btn-sm" data-kt-menu-trigger="click" data-kt-menu-placement="bottom-end" data-kt-menu-flip="top-end">
                Actions
                <span class="svg-icon fs-5 m-0">
                    <svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="24px" height="24px" viewBox="0 0 24 24" version="1.1">
                        <g stroke="none" stroke-width="1" fill="none" fill-rule="evenodd">
                            <polygon points="0 0 24 0 24 24 0 24"></polygon>
                            <path d="M6.70710678,15.7071068 C6.31658249,16.0976311 5.68341751,16.0976311 5.29289322,15.7071068 C4.90236893,15.3165825 4.90236893,14.6834175 5.29289322,14.2928932 L11.2928932,8.29289322 C11.6714722,7.91431428 12.2810586,7.90106866 12.6757246,8.26284586 L18.6757246,13.7628459 C19.0828436,14.1360383 19.1103465,14.7686056 18.7371541,15.1757246 C18.3639617,15.5828436 17.7313944,15.6103465 17.3242754,15.2371541 L12.0300757,10.3841378 L6.70710678,15.7071068 Z" fill="currentColor" fill-rule="nonzero" transform="translate(12.000003, 11.999999) rotate(-180.000000) translate(-12.000003, -11.999999)"></path>
                        </g>
                    </svg>
                </span>
            </a>
            <!--begin::Menu-->
            <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-600 menu-state-bg-light-primary fw-bold fs-7 w-125px py-4" data-kt-menu="true">
                <!--begin::Menu item-->
                <div class="menu-item px-3">
                    <a href="/Books/Edit/${row.id}" class="menu-link px-3 " >
                        Edit
                    </a>
                </div>
                <!--end::Menu item-->
                <!--begin::Menu item-->
                <div class="menu-item px-3">
                    <a  class="menu-link px-3 js-toggle-status" data-urltoggle ="/Books/ToggleStatus/${row.id}">
                        Toggle Status
                    </a>
                </div>
                <!--end::Menu item-->
            </div>
            <!--end::Menu-->

        </div>`

                }
            },
        ]
    });



});


