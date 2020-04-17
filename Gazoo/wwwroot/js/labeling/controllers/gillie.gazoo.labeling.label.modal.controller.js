(function () {
    'use strict';

    angular
        .module('Company.gazoo.app')
        .controller('labelingLabelModalController', labelingLabelModalController);

    labelingLabelModalController.$inject = ['$scope', '$uibModalInstance', 'formData'];

    function labelingLabelModalController($scope, $uibModalInstance, formData) {
        $scope.title = formData.title;

        $scope.name = "";

        if (formData.label) {
            $scope.name = formData.label.name;
        }

        $scope.save = function (form) {
            if (!form.$valid)
                return;

            formData.checkLabelNameExists($scope.name, function (result) {
                if (result.data && (formData.label ? $scope.name !== formData.label.name : true)) {
                    form.title.$error.exist = true;
                    return;
                }
                $uibModalInstance.close();
                var label = {
                    name: $scope.name,
                    id: formData.label ? formData.label.id : 0
                };
                if (formData.onSave) {
                    formData.onSave(label);
                }
            });
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');

            if (formData.onCancel)
                formData.onCancel();
        };
    }
})();