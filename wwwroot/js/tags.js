document.addEventListener("DOMContentLoaded", function() {
    var filters = {};
    // Проверяем, есть ли данные в sessionStorage
    if (sessionStorage.getItem('filtersData')) {
        // Если есть, извлекаем и десериализуем их
        filters = JSON.parse(sessionStorage.getItem('filtersData'));
    }

    var grid = document.querySelector('.grid');
    var iso = new Isotope(grid, {
        itemSelector: '.col'
    });



    function combineFilters() {
        var output = [];
        var keys = Object.keys(filters);

        function recurse(currIndex, currFilter) {
            if (currIndex === keys.length) {
                output.push(currFilter);
                return;
            }

            for (var i = 0; i < filters[keys[currIndex]].length; i++) {
                recurse(currIndex + 1, currFilter + filters[keys[currIndex]][i]);
            }
        }

        if (keys.length > 0) {
            recurse(0, "");
        }

        return output.length > 0 ? output.join(",") : "*";
    }

    function applyFilters() {
        var filterValue = combineFilters();
        iso.arrange({ filter: filterValue });
    }
    function updateFilterCount() {
        var count = Object.keys(filters).reduce((acc, key) => acc + filters[key].length, 0);
        document.querySelector('.filter-count-badge').textContent = count;
    }

    var checkboxes = document.querySelectorAll('.dropdown-menu .filter-checkbox');
    checkboxes.forEach(function(checkbox) {
        var group = checkbox.dataset.group;
        var filterValue = checkbox.dataset.filter;
        if (filters[group] && filters[group].includes(filterValue)) {
            checkbox.checked = true;
            checkbox.parentElement.parentElement.classList.add('selected-filter');
        }
        checkbox.addEventListener('change', function() {
            var group = this.dataset.group;
            var filterValue = this.dataset.filter;

            if (!this.checked) {
                var index = filters[group].indexOf(filterValue);
                if (index > -1) {
                    filters[group].splice(index, 1);
                    if (!filters[group].length) {
                        delete filters[group];
                    }
                }
                this.parentElement.parentElement.classList.remove('selected-filter');
            } else {
                if (!filters[group]) {
                    filters[group] = [];
                }
                filters[group].push(filterValue);
                this.parentElement.parentElement.classList.add('selected-filter');
            }
            applyFilters();
            // Сохраняем данные фильтрации в sessionStorage после применения фильтров
            var filtersString = JSON.stringify(filters);
            sessionStorage.setItem('filtersData', filtersString);
            updateFilterCount();
        });
    });
    updateFilterCount();
    applyFilters()

    var allButton = document.getElementById('all');
    allButton.addEventListener('click', function() {
        checkboxes.forEach(function(checkbox) {
            checkbox.checked = false;
            checkbox.parentElement.parentElement.classList.remove('selected-filter');
        });
        filters = {};
        // Сохраняем данные фильтрации в sessionStorage после применения фильтров
        var filtersString = JSON.stringify(filters);
        sessionStorage.setItem('filtersData', filtersString);
        updateFilterCount();
        applyFilters();
    });
    
});
