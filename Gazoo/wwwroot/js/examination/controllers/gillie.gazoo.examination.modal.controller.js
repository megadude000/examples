(function () {
    'use strict';

    angular
        .module('Company.gazoo.app')
        .controller('examinationModalController', examinationModalController);

    examinationModalController.$inject = ['$q', '$scope', '$uibModalInstance', 'promiseCommonService', 'formData'];

    function examinationModalController($q, $scope, $uibModalInstance, promiseCommonService, formData) {
        $scope.title = formData.title;
        $scope.answerSets = formData.answerSets;
        $scope.examination = {
            name: "",
            reactionTime: 0,
            predefinedAnswerSetId: null
        };
        $scope.save = function (form) {
            if (!form.$valid)
                return;

            $uibModalInstance.close();

            if (formData.onSave) {
                $scope.examination.predefinedAnswerSetId = $scope.examination.predefinedAnswerSetId === 0 ? null : $scope.examination.predefinedAnswerSetId;
                formData.onSave($scope.examination);
            }
        };

        $scope.nameChanged = function () {
            formData.checkExaminationNameExists($scope.examination.name, function (result) {
                if (result.data) {
                    $scope.examinationForm.title.$setValidity("validName", false);
                    return;
                }

                $scope.examinationForm.title.$setValidity("validName", true);
            });
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');

            if (formData.onCancel)
                formData.onCancel();
        };
    }
})();