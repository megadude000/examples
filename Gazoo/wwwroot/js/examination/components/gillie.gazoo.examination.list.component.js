(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .controller('examinationListController', examinationListController);

    angular
        .module('Company.gazoo.examination')
        .component('examinationList', {
            controller: 'examinationListController',
            templateUrl: '/templates/examination/examinationList.html'
        });

    examinationListController.$inject = ['$scope', '$state', '$translate', 'examinationHttpService', 'promiseCommonService', 'orderByFilter', '$uibModal', 'moment', '$ngBootbox'];

    function examinationListController($scope, $state, $translate, examinationHttpService, promiseCommonService, orderByFilter, $uibModal, moment, $ngBootbox) {
        var examinations = [];
        var formData = {};
        var answerSets = [];
        $scope.propertyName = 'dateOfCreation';
        $scope.reverse = true;
        $scope.examinations = [];
        $scope.paginatedExaminations = [];
        $scope.startDateOptions;
        $scope.endDateOptions;

        $scope.pagination = {
            currentPage: 1,
            itemsPerPage: 10,
            totalItems: 0,
            maxVisiblePages: 5
        };

        $scope.startDatePopup = {
            opened: false
        };

        $scope.endDatePopup = {
            opened: false
        };

        $scope.openStartDatePopup = function () {
            $scope.startDatePopup.opened = true;
        };

        $scope.openEndDatePopup = function () {
            $scope.endDatePopup.opened = true;
        };

        $scope.changeMinEndTime = function () {
            $scope.endDateOptions.minDate = $scope.filterModel.startDate;
        };

        $scope.changeMaxStartTime = function () {
            $scope.startDateOptions.maxDate = $scope.filterModel.endDate;
        };

        $scope.initialize = function () {
            formData.checkExaminationNameExists = checkExaminationNameExists;
            formData.onSave = saveExamination;

            createFilterModel();
            $scope.filterExaminations();
            getPredefinedAnswerSets();
        };

        $scope.resetFilter = function () {
            $scope.endDateOptions.minDate = null;
            $scope.startDateOptions.maxDate = null;
            $scope.propertyName = 'dateOfcreation';
            $scope.reverse = true;
            $scope.pagination.currentPage = 1;

            createFilterModel();
            $scope.filterExaminations();
            getPredefinedAnswerSets();
        };

        $scope.filterExaminations = function () {
            prepareFilterModel();
            $scope.fetchExaminationsPromise = promiseCommonService.createPromise(
                examinationHttpService.getFilteredExaminations,
                $scope.preparedFilterModel,
                "ERROR_DURING_FETCHING_EXAMINATIONS",
                function (res) {
                    examinations = res.data;
                    examinations.forEach(exam => {
                        exam.creationTime = moment.utc(exam.creationTime).local().format("HH:mm DD.MM.YYYY");
                        exam.modificationTime = moment.utc(exam.modificationTime).local().format("HH:mm DD.MM.YYYY");
                    });
                    $scope.pagination.totalItems = res.data.length;
                    $scope.examinations = orderByFilter(examinations, $scope.propertyName, $scope.reverse);
                    $scope.setPage();
                });
        };

        $scope.goToEditTab = function (examId) {
            var url = $state.href("examinationDetails", { "examId": examId });
            window.open(url, '_blank');
        };

        $scope.addExamination = function () {
            formData.title = $translate.instant('ADD_EXAMINATION');
            formData.userData = {};
            formData.answerSets = answerSets;

            openAddExaminationModelForm(formData);
        };

        $scope.confirmDelete = function (exam) {
            $ngBootbox.confirm($translate.instant("QUESTION_DELETE_FILES_CONFIRMATION"))
                .then(function () {
                    deleteExam(exam);
                });
        };

        $scope.sortBy = function (propertyName) {
            $scope.reverse = (propertyName !== null && $scope.propertyName === propertyName)
                ? !$scope.reverse : false;
            $scope.propertyName = propertyName;
            $scope.examinations = orderByFilter(examinations, $scope.propertyName, $scope.reverse);
            $scope.pagination.currentPage = 1;
            $scope.setPage();
        };

        $scope.setPage = function () {
            var skip = ($scope.pagination.currentPage - 1) * $scope.pagination.itemsPerPage;
            $scope.paginatedExaminations = $scope.examinations.slice(skip, skip + $scope.pagination.itemsPerPage);
        };

        function saveExamination(filledModel) {
            $scope.addExaminationPromise = promiseCommonService.createPromise(
                examinationHttpService.addExamination,
                filledModel,
                "ERROR_DURING_ADDING_EXAMINATION",
                function () {
                    $scope.pagination.currentPage = 1;
                    $scope.filterExaminations();
                });
        }

        function checkExaminationNameExists(examTitle, func) {
            return promiseCommonService.createPromise(
                examinationHttpService.checkExaminationNameExists,
                examTitle,
                "ALERT_ERROR_TITLE",
                func);
        }

        function openAddExaminationModelForm(formData) {
            $uibModal.open({
                animation: true,
                templateUrl: 'templates/examination/examinationModalTemplate.html',
                controller: 'examinationModalController',
                size: "lg",
                resolve: {
                    formData: formData
                }
            });
        }

        function deleteExam(exam) {
            $scope.deleteExaminationPromise = promiseCommonService.createPromise(
                examinationHttpService.deleteExamination,
                exam.id,
                "ERROR_DURING_DELETING_EXAMINATION",
                function () {
                    $scope.filterExaminations();
                });
        }

        function createFilterModel() {
            $scope.filterModel = {
                examinationTitle: "",
                startDate: null,
                endDate: null
            };
        }

        function prepareFilterModel() {
            $scope.preparedFilterModel = {
                examinationTitle: $scope.filterModel.examinationTitle,
                startDate: $scope.filterModel.startDate !== null ? moment($scope.filterModel.startDate).format("L") : null,
                endDate: $scope.filterModel.endDate !== null ? moment($scope.filterModel.endDate).format("L") : null
            };
        }

        function getPredefinedAnswerSets() {
            $scope.getPredefinedAnswerSetsPromise = promiseCommonService.createPromise(
                examinationHttpService.getPredefinedAnswerSets,
                null,
                "ERROR_DURING_DETTING_PREDEFINED_ANSWER_SETS",
                function (response) {
                    answerSets = response.data;
                    answerSets.unshift({ id: 0, name: $translate.instant('DEFAULT_SET_LABEL')});
                }
            );
        }
    }
})();