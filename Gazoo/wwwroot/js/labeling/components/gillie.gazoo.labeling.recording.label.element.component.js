(() => {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .component('labelElement', {
            controller: 'recordingLabelElementController',
            templateUrl: '/templates/labeling/recordingLabelElement.html',
            bindings: {
                labelGroup: "<",
                log: "<",
                onChange: "&"
            }
        });

    angular
        .module('Company.gazoo.labeling')
        .controller('recordingLabelElementController', recordingLabelElementController);

    recordingLabelElementController.$inject = ['$scope'];

    function recordingLabelElementController($scope) {
        $scope.labelElementType = LabelElementType;
        $scope.element = {
            multiselectSettings: {
                idProp: 'id',
                displayProp: 'name',
                template: '{{option.name}}',
                scrollableHeight: 'auto',
                scrollable: true,
                selectAll: true
            },
            values: [],
        }

        $scope.init = () => {
            $scope.label = this.labelGroup;
            $scope.element.options = this.labelGroup.options;

            var lastSavedValues = this.log.labels.find(label => label.id === this.labelGroup.id).values;
            if (lastSavedValues.length) {
                if (this.labelGroup.type === LabelElementType.DropdownMultiselect)
                    $scope.element.values = lastSavedValues.map(labelId => ({ "id": labelId }));

                if (this.labelGroup.type === LabelElementType.Dropdown)
                    $scope.element.values = lastSavedValues[0];

                if (this.labelGroup.type === LabelElementType.Checkbox)
                    $scope.element.values = $scope.label.options.find(option => option.id === lastSavedValues[0]).id;
            }
        };

        $scope.onChange = () => {
            this.onChange({
                logId: this.log.id,
                labelGroupId: this.labelGroup.id,
                values: this.labelGroup.type !== LabelElementType.DropdownMultiselect ? $scope.element.values : $scope.element.values.map(value => value.id),
            });
        };
    }
})();