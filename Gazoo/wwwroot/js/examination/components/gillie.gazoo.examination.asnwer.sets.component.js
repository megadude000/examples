(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .controller('answerSetsController', answerSetsController);

    angular
        .module('Company.gazoo.examination')
        .component('answerSetsComponent', {
            controller: 'answerSetsController',
            templateUrl: '/templates/examination/answerSets.html'
        });

    answerSetsController.$inject = ['$scope', 'examinationHttpService', 'alertService', 'promiseCommonService', '$translate', '$uibModal', '$ngBootbox'];

    function answerSetsController($scope, examinationHttpService, alertService, promiseCommonService, $translate, $uibModal, $ngBootbox) {
        var formData = {};
        $scope.answerType = AnswerType;
        $scope.answerSets = [];
        $scope.selectedAnswerSet;

        $scope.initialize = () => {
            getPredefinedAnswerSets();
        };

        $scope.addAnswerSet = () => {
            formData = {};

            formData.answerSetModel = {
                id: null,
                name: null
            };

            formData.title = $translate.instant('ADD_PREDEFINED_ANSWER_SET_LABEL');
            formData.checkAnswerSetNameExists = checkAnswerSetNameExists;
            formData.onSave = saveAnswerSet;

            openAnswerSetModelForm(formData);
        };

        $scope.editAnswerSet = (answerSet) => {
            formData = {};

            formData.answerSetModel = {
                id: answerSet.id,
                name: $scope.getTranslationIfNeed(answerSet.name)
            };

            formData.title = $translate.instant('EDIT_PREDEFINED_ANSWER_SET_LABEL');
            formData.checkAnswerSetNameExists = checkAnswerSetNameExists;
            formData.onSave = updateAnswerSet;

            openAnswerSetModelForm(formData);
        };

        $scope.setAnswers = (id) => {
            $scope.selectedAnswerSet = id;
        };

        $scope.confirmDeleteAnswerSet = function (answerSetId) {
            $ngBootbox.confirm($translate.instant("ANSWER_SET_DELETE_CONFIRMATION"))
                .then(function () {
                    checkAnswerSetIsUsed(answerSetId);
                });
        };

        $scope.getTranslationIfNeed = (name) => {
            if (name.indexOf('_LABEL') !== -1)
                return $translate.instant(name);

            return name;
        };

        function checkAnswerSetIsUsed(answerSetId) {
            $scope.checkIsAnswerUsingPromise = promiseCommonService.createPromise(
                examinationHttpService.checkAnswerSetIsUsed,
                answerSetId,
                "ALERT_ERROR_TITLE",
                function (response) {
                    if (response.data) {
                        alertService.showWarning("ERROR_ANSWER_SET_IS_USED");
                        return;
                    }

                    deleteAnswerSet(answerSetId);
                });
        }

        function deleteAnswerSet(answerSetId) {
            $scope.deleteAnswerPromise = promiseCommonService.createPromise(
                examinationHttpService.deleteAnswerSet,
                answerSetId,
                "ERROR_DURING_DELETING_ANSWER_SET",
                function (response) {
                    getPredefinedAnswerSets();
                });
        }

        function openAnswerSetModelForm(formData) {
            $uibModal.open({
                animation: true,
                templateUrl: 'templates/examination/answerSetModalTemplate.html',
                controller: 'answerSetModalController',
                size: "md",
                resolve: {
                    formData: formData
                }
            });
        }

        function checkAnswerSetNameExists(name, func) {
            return promiseCommonService.createPromise(
                examinationHttpService.checkAnswerSetNameExists,
                name,
                "ALERT_ERROR_TITLE",
                func);
        }

        function saveAnswerSet(request) {
            $scope.answerSetPromise = promiseCommonService.createPromise(
                examinationHttpService.addAnswerSet,
                request,
                "ERROR_DURING_ADDING_PREDEFINED_ANSWER_SETS",
                function () {
                    getPredefinedAnswerSets();
                });
        }

        function updateAnswerSet(request) {
            $scope.answerSetPromise = promiseCommonService.createPromise(
                examinationHttpService.updateAnswerSet,
                request,
                "ERROR_DURING_UPDATING_PREDEFINED_ANSWER_SETS",
                function () {
                    getPredefinedAnswerSets();
                });
        }

        function getPredefinedAnswerSets() {
            $scope.getPredefinedAnswerSetsPromise = promiseCommonService.createPromise(
                examinationHttpService.getPredefinedAnswerSets,
                null,
                "ERROR_DURING_DETTING_PREDEFINED_ANSWER_SETS",
                function (response) {
                    $scope.answerSets = response.data;
                    if ($scope.answerSets.length && $scope.selectedAnswerSet === undefined)
                        $scope.setAnswers($scope.answerSets[0].id);
                }
            );
        }
    }
})();