// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {
    // Explicitly initialize all MDB dropdowns
    document.querySelectorAll('.dropdown-toggle').forEach(function (dropdown) {
        new mdb.Dropdown(dropdown);
    });
    // Explicitly initialize the navbar collapse
    const collapsibleElement = document.getElementById('navbarSupportedContent');
    if (collapsibleElement) {
        new mdb.Collapse(collapsibleElement, { toggle: false });
    }
});
