import React, { useState } from 'react';
import { Modal, Button } from 'react-bootstrap';

interface CustomModalProps {
    show: boolean;
    handleClose: () => void;
    title: string;
    message: string;
}

const CustomModal: React.FC<CustomModalProps> = ({ show, handleClose, title, message }) => {
    return (
        <Modal show={show} onHide={handleClose}>
            <Modal.Header closeButton>
                <Modal.Title>{title}</Modal.Title>
            </Modal.Header>
            <Modal.Body>{message}</Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={handleClose}>
                    Close
                </Button>
            </Modal.Footer>
        </Modal>
    );
};

export default CustomModal;