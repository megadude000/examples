(function () {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .directive('labels', labels);

    labels.$inject = [];

    function labels() {
        return {
            scope: {
                data: '=bind',
                selected: '='
            },
            templateUrl: 'templates/labeling/labelsDirective.html',
            controller: function ($scope) {
                $scope.data.selectedLabels = [];
                $scope.labelGroupName = $scope.data.labelGroup.name;

                $scope.setSelected = function () {
                    if ($scope.selected && $scope.selected.length !== 0) {
                        $scope.data.labels.forEach(function (label) {
                            if ($scope.selected.some(i => i === label.id))
                                label.isSelected = true;
                            else
                                label.isSelected = false;
                        });
                    }
                };

                $scope.unselectExcept = function (id) {
                    $scope.data.labels.forEach(function (label) {
                        if (label.id !== id)
                            label.isSelected = false;
                    });
                };

                $scope.refresh = function () {
                    $scope.data.labels.forEach(function (label) {
                        label.isSelected = false;
                    });
                };
            }
        };
    }
})();