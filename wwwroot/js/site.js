// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {
    // Initialize MDBootstrap components

    // Initialize Dropdowns
    document.querySelectorAll('.dropdown-toggle').forEach(function (dropdown) {
        new mdb.Dropdown(dropdown);
    });

    // Initialize Collapse
    const collapsibleElement = document.getElementById('navbarSupportedContent');
    if (collapsibleElement) {
        new mdb.Collapse(collapsibleElement, { toggle: false });
    }

    // Initialize Inputs (form-outline)
    document.querySelectorAll('.form-outline').forEach((formOutline) => {
        new mdb.Input(formOutline).init();
    });

    // Initialize Selects
    document.querySelectorAll('[data-mdb-select-init]').forEach((select) => {
        new mdb.Select(select);
    });

    // Initialize Datepickers
    document.querySelectorAll('.datepicker').forEach((datepicker) => {
        new mdb.Datepicker(datepicker);
    });

    // Initialize Modals
    document.querySelectorAll('.modal').forEach((modal) => {
        new mdb.Modal(modal);
    });
});


