(function () {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .controller('labelingLabelsCreationToolController', labelingLabelsCreationToolController);

    angular
        .module('Company.gazoo.labeling')
        .component('masterActionsModule.labelsCreationTool', {
            controller: 'labelingLabelsCreationToolController',
            templateUrl: '/templates/labeling/labelsCreationTool.html'
        });

    labelingLabelsCreationToolController.$inject = ['$scope', '$uibModal', '$translate', '$ngBootbox', 'transacriptionHttpService', 'promiseCommonService'];

    function labelingLabelsCreationToolController($scope, $uibModal, $translate, $ngBootbox, transacriptionHttpService, promiseCommonService) {
        var formData = {};

        $scope.groupId = null;
        $scope.labelGroups = [];
        $scope.labels = [];

        $scope.initialize = function () {
            $scope.getLabelGroups();
        };

        $scope.getLabelGroups = function () {
            $scope.getLabelGroupsPromise = promiseCommonService.createPromise(
                transacriptionHttpService.getLabelGroups,
                null,
                "ERROR",
                function (response) {
                    $scope.labelGroups = response.data;

                    if (!$scope.groupId)
                        $scope.setGroup($scope.labelGroups[0].id);

                    $scope.getLabels($scope.groupId);
                });
        };

        $scope.setGroup = function (groupId) {
            $scope.groupId = groupId;
            $scope.getLabels($scope.groupId);
        };

        $scope.getLabels = function (id) {
            $scope.getLabelGroupsPromise = promiseCommonService.createPromise(
                transacriptionHttpService.getLabels,
                id,
                "ERROR",
                function (response) {
                    $scope.labels = response.data;
                });
        };

        $scope.editGroup = function (group) {
            formData = {};
            formData.title = $translate.instant('EDIT_LABEL_GROUP');
            formData.onSave = updateLabelGroup;
            formData.checkLabelGroupNameExists = checkLabelGroupNameExists;
            formData.labelGroup = angular.copy(group);
            formData.labelGroup.type = String(group.type);

            openAddGroupModelForm(formData);
        };

        $scope.editLabel = function (label) {
            formData = {};
            formData.title = $translate.instant('EDIT_LABEL');
            formData.onSave = updateLabel;
            formData.checkLabelNameExists = checkLabelNameExists;
            formData.label = angular.copy(label);

            openAddLabelModelForm(formData);
        };

        $scope.addGroup = function () {
            formData = {};
            formData.title = $translate.instant('ADD_LABEL_GROUP');
            formData.checkLabelGroupNameExists = checkLabelGroupNameExists;
            formData.onSave = saveLabelGroup;

            openAddGroupModelForm(formData);
        };

        $scope.addLabel = function () {
            formData = {};
            formData.title = $translate.instant('ADD_LABEL');
            formData.checkLabelNameExists = checkLabelNameExists;
            formData.onSave = saveLabel;

            openAddLabelModelForm(formData);
        };

        $scope.deleteGroup = function (group) {
            $ngBootbox.confirm($translate.instant("CONFIRMATION"))
                .then(function () {
                    if ($scope.groupId === group.id)
                        $scope.groupId = null;

                    deleteLabelGroup(group.id);
                });
        };

        $scope.deleteLabel = function (label) {
            $ngBootbox.confirm($translate.instant("CONFIRMATION"))
                .then(function () {
                    deleteLabel(label.id);
                });
        };

        $scope.canAddOpiton = (groupId) => {
            var groupType = $scope.labelGroups.find(group => group.id === groupId).type;
            return groupType === LabelElementType.Dropdown || groupType === LabelElementType.DropdownMultiselect;
        };

        function openAddGroupModelForm(formData) {
            $uibModal.open({
                animation: true,
                templateUrl: 'templates/labeling/labelGroupModalTemplate.html',
                controller: 'labelingLabelGroupModalController',
                size: "md",
                resolve: {
                    formData: formData
                }
            });
        }

        function openAddLabelModelForm(formData) {
            $uibModal.open({
                animation: true,
                templateUrl: 'templates/labeling/labelModalTemplate.html',
                controller: 'labelingLabelModalController',
                size: "md",
                resolve: {
                    formData: formData
                }
            });
        }

        function saveLabelGroup(filledModel) {
            $scope.labelGroupPromise = promiseCommonService.createPromise(
                transacriptionHttpService.addLabelGroup,
                filledModel,
                "ERROR",
                function () {
                    $scope.getLabelGroups();
                });
        }

        function deleteLabelGroup(labelGroupId) {
            $scope.labelGroupPromise = promiseCommonService.createPromise(
                transacriptionHttpService.deleteLabelGroup,
                labelGroupId,
                "ERROR",
                function () {
                    $scope.getLabelGroups();
                });
        }

        function deleteLabel(labelId) {
            $scope.labelGroupPromise = promiseCommonService.createPromise(
                transacriptionHttpService.deleteLabel,
                labelId,
                "ERROR",
                function () {
                    $scope.getLabelGroups();
                });
        }

        function saveLabel(filledModel) {
            filledModel.labelGroupId = $scope.groupId;
            $scope.labelPromise = promiseCommonService.createPromise(
                transacriptionHttpService.addLabel,
                filledModel,
                "ERROR",
                function () {
                    $scope.getLabelGroups();
                });
        }

        function updateLabelGroup(filledModel) {
            $scope.labelGroupPromise = promiseCommonService.createPromise(
                transacriptionHttpService.updateLabelGroup,
                filledModel,
                "ERROR",
                function () {
                    $scope.getLabelGroups();
                });
        }

        function updateLabel(filledModel) {
            filledModel.labelGroupId = $scope.groupId;
            $scope.labelPromise = promiseCommonService.createPromise(
                transacriptionHttpService.updateLabel,
                filledModel,
                "ERROR",
                function () {
                    $scope.getLabelGroups();
                });
        }

        function checkLabelGroupNameExists(title, func) {
            return promiseCommonService.createPromise(
                transacriptionHttpService.checkLabelGroupNameExists,
                title,
                "ALERT_ERROR_TITLE",
                func);
        }

        function checkLabelNameExists(title, func) {
            return promiseCommonService.createPromise(
                transacriptionHttpService.checkLabelNameExists,
                title + '/' + $scope.groupId,
                "ALERT_ERROR_TITLE",
                func);
        }
    }
})();