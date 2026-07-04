(function () {
    'use strict';

    var MIN_SCALE = 0.5;
    var MAX_SCALE = 5;
    var ZOOM_STEP = 0.25;
    var PAN_STEP = 80;

    function initQuestionImageViewer() {
        if (typeof bootstrap === 'undefined') return;

        var modalEl = document.getElementById('questionImageModal');
        var viewport = document.getElementById('questionImageViewport');
        var modalImg = document.getElementById('questionImageModalImg');
        var modalTitle = document.getElementById('questionImageModalLabel');
        var zoomLabel = document.getElementById('questionImageZoomLabel');
        var btnZoomIn = document.getElementById('questionImageZoomIn');
        var btnZoomOut = document.getElementById('questionImageZoomOut');
        var btnPanLeft = document.getElementById('questionImagePanLeft');
        var btnPanRight = document.getElementById('questionImagePanRight');
        var btnReset = document.getElementById('questionImageReset');

        if (!modalEl || !viewport || !modalImg) return;

        var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        var scale = 1;
        var translateX = 0;
        var translateY = 0;
        var isDragging = false;
        var dragStartX = 0;
        var dragStartY = 0;
        var dragOriginX = 0;
        var dragOriginY = 0;

        function updateZoomLabel() {
            if (zoomLabel) {
                zoomLabel.textContent = Math.round(scale * 100) + '%';
            }
        }

        function applyTransform() {
            modalImg.style.transform = 'translate(' + translateX + 'px, ' + translateY + 'px) scale(' + scale + ')';
            updateZoomLabel();
        }

        function resetTransform() {
            scale = 1;
            translateX = 0;
            translateY = 0;
            applyTransform();
        }

        function clampScale(value) {
            return Math.min(MAX_SCALE, Math.max(MIN_SCALE, value));
        }

        function zoom(delta) {
            scale = clampScale(scale + delta);
            applyTransform();
        }

        function pan(dx, dy) {
            translateX += dx;
            translateY += dy;
            applyTransform();
        }

        function openModal(src, alt) {
            resetTransform();
            modalImg.src = src || '';
            modalImg.alt = alt || 'Ảnh minh họa';
            if (modalTitle) modalTitle.textContent = alt || 'Ảnh minh họa';
            modal.show();
        }

        document.addEventListener('click', function (event) {
            var trigger = event.target.closest('.js-question-image-open');
            if (!trigger) return;
            event.preventDefault();
            openModal(trigger.dataset.imageSrc, trigger.dataset.imageAlt);
        });

        if (btnZoomIn) btnZoomIn.addEventListener('click', function () { zoom(ZOOM_STEP); });
        if (btnZoomOut) btnZoomOut.addEventListener('click', function () { zoom(-ZOOM_STEP); });
        if (btnPanLeft) btnPanLeft.addEventListener('click', function () { pan(-PAN_STEP, 0); });
        if (btnPanRight) btnPanRight.addEventListener('click', function () { pan(PAN_STEP, 0); });
        if (btnReset) btnReset.addEventListener('click', resetTransform);

        viewport.addEventListener('mousedown', function (event) {
            if (event.button !== 0) return;
            isDragging = true;
            dragStartX = event.clientX;
            dragStartY = event.clientY;
            dragOriginX = translateX;
            dragOriginY = translateY;
            viewport.classList.add('is-dragging');
            event.preventDefault();
        });

        window.addEventListener('mousemove', function (event) {
            if (!isDragging) return;
            translateX = dragOriginX + (event.clientX - dragStartX);
            translateY = dragOriginY + (event.clientY - dragStartY);
            applyTransform();
        });

        window.addEventListener('mouseup', function () {
            if (!isDragging) return;
            isDragging = false;
            viewport.classList.remove('is-dragging');
        });

        viewport.addEventListener('wheel', function (event) {
            event.preventDefault();
            zoom(event.deltaY < 0 ? ZOOM_STEP : -ZOOM_STEP);
        }, { passive: false });

        modalEl.addEventListener('hidden.bs.modal', function () {
            resetTransform();
            modalImg.src = '';
            isDragging = false;
            viewport.classList.remove('is-dragging');
        });
    }

    if (document.readyState === 'complete') {
        initQuestionImageViewer();
    } else {
        window.addEventListener('load', initQuestionImageViewer);
    }
})();
