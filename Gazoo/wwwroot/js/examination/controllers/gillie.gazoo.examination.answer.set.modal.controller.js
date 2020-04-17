(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .controller('answerSetModalController', answerSetModalController);

    answerSetModalController.$inject = ['$scope', '$uibModalInstance', 'formData'];

    function answerSetModalController($scope, $uibModalInstance, formData) {
        $scope.title = formData.title;
        $scope.initialAnswerSetModel = formData.answerSetModel;
        $scope.answerSetModel = angular.copy(formData.answerSetModel);

        $scope.save = (form) => {
            if (!form.$valid)
                return;

            if (formData.onSave) {
                $uibModalInstance.close();

                var request = formRequest();

                formData.onSave(request);
            }
        };

        $scope.cancel = () => {
            $uibModalInstance.dismiss('cancel');
        };

        $scope.nameChanged = function () {
            formData.checkAnswerSetNameExists($scope.answerSetModel.name, function (result) {
                if (result.data && initialAnswerSetModel.name !== $scope.answerSetModel.name) {
                    $scope.answerSetForm.answerSetName.$setValidity("validName", false);
                    return;
                }

                $scope.answerSetForm.answerSetName.$setValidity("validName", true);
            });
        };

        function formRequest() {
            var request = {
                name: $scope.answerSetModel.name
            };

            if ($scope.answerSetModel.id)
                request.id = $scope.answerSetModel.id;

            return request;
        }
    }
})();