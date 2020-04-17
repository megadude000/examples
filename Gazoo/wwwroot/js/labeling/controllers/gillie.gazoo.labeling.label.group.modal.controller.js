(function () {
    'use strict';

    angular
        .module('Company.gazoo.app')
        .controller('labelingLabelGroupModalController', labelingLabelGroupModalController);

    labelingLabelGroupModalController.$inject = ['$scope', '$translate', '$uibModalInstance', '$ngBootbox', 'formData', 'mapService'];

    function labelingLabelGroupModalController($scope, $translate, $uibModalInstance, $ngBootbox, formData, mapService) {
        $scope.title = formData.title;

        $scope.elementTypes = mapService.getArrayFromMap(mapService.getInvertedMap(LabelElementType));
        $scope.name = "";
        $scope.multipleSelect = false;
        $scope.type;

        var oryginalType;

        if (formData.labelGroup) {
            $scope.name = formData.labelGroup.name;
            $scope.multipleSelect = formData.labelGroup.multipleSelect;
            $scope.type = oryginalType = formData.labelGroup.type;
        }

        $scope.save = function (form) {
            if (!form.$valid)
                return;

            formData.checkLabelGroupNameExists($scope.name, (result) => {
                if (result.data && (formData.labelGroup ? $scope.name !== formData.labelGroup.name : true)) {
                    form.title.$error.exist = true;
                    return;
                }
                if (oryginalType && $scope.type !== oryginalType && $scope.type === String(LabelElementType.Checkbox)) {
                    $ngBootbox.confirm($translate.instant("QUESTION_CHANGE_ELEMENT_TYPE_TO_CHECKBOX"))
                        .then(() => {
                            saveLabelGroup();
                        });
                }
                else {
                    saveLabelGroup();
                }
            });
        };

        $scope.setElementTypeByMultipleSelectCheckbox = (multipleSelect) => {
            if (multipleSelect)
                $scope.type = String(LabelElementType.DropdownMultiselect);
            else
                $scope.type = String(LabelElementType.Dropdown);
        };

        $scope.setMultipleSelectCheckboxByElementType = (type) => {
            if (type === String(LabelElementType.DropdownMultiselect))
                $scope.multipleSelect = true;
            else
                $scope.multipleSelect = false;
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');

            if (formData.onCancel)
                formData.onCancel();
        };

        function saveLabelGroup() {
            $uibModalInstance.close();
            if (formData.onSave) {
                var labelGroup = {
                    name: $scope.name,
                    multipleSelect: $scope.multipleSelect,
                    type: $scope.type,
                    id: formData.labelGroup ? formData.labelGroup.id : 0
                };
                formData.onSave(labelGroup);
            }
        }
    }
})();