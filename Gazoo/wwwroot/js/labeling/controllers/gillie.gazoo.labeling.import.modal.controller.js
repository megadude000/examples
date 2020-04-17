(function () {
    'use strict';

    angular
        .module('Company.gazoo.app')
        .controller('importModalController', importModalController);

    importModalController.$inject = ['$scope', '$uibModalInstance', 'formData'];

    function importModalController($scope, $uibModalInstance, formData) {
        $scope.title = formData.title;
        $scope.importModel = formData.importModel;
        $scope.priorityLevels = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        $scope.save = function (form) {
            if (!form.$valid)
                return;

            if (formData.onSave) {
                $uibModalInstance.close();

                var request = {
                    id: $scope.importModel.id,
                    priority: $scope.importModel.priority,
                    comment: $scope.importModel.comment
                };

                formData.onSave(request);
            }
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');

            if (formData.onCancel)
                formData.onCancel();
        };
    }
})();