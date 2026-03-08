import React, { useEffect, useState } from 'react';
import { Table, Card, Button, Typography, Space, message, Tag, Modal, Form, Input } from 'antd';
import { ReloadOutlined, PlusOutlined, RocketOutlined } from '@ant-design/icons';
import api from '../api/axios';
import type { Workflow } from '../types';

const { Title } = Typography;

const WorkflowsPage: React.FC = () => {
    const [workflows, setWorkflows] = useState<Workflow[]>([]);
    const [loading, setLoading] = useState(false);
    const [isModalVisible, setIsModalVisible] = useState(false);
    const [editingWorkflow, setEditingWorkflow] = useState<Workflow | null>(null);
    const [confirmLoading, setConfirmLoading] = useState(false);
    const [form] = Form.useForm();

    const fetchWorkflows = async () => {
        setLoading(true);
        try {
            const response = await api.get('/workflows');
            setWorkflows(response.data);
        } catch (error) {
            message.error('Không thể tải danh sách workflows.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchWorkflows();
    }, []);

    const handleSaveWorkflow = async (values: any) => {
        setConfirmLoading(true);
        try {
            if (editingWorkflow) {
                await api.put(`/workflows/${editingWorkflow.id}`, { ...values, isActive: editingWorkflow.isActive });
                message.success('Cập nhật workflow thành công!');
            } else {
                await api.post('/workflows', { ...values, isActive: true });
                message.success('Thêm workflow thành công!');
            }
            setIsModalVisible(false);
            setEditingWorkflow(null);
            form.resetFields();
            fetchWorkflows();
        } catch (error: any) {
            message.error(error.response?.data?.message || 'Thao tác thất bại.');
        } finally {
            setConfirmLoading(false);
        }
    };

    const handleOpenAdd = () => {
        setEditingWorkflow(null);
        form.resetFields();
        setIsModalVisible(true);
    };

    const handleOpenEdit = (workflow: Workflow) => {
        setEditingWorkflow(workflow);
        form.setFieldsValue(workflow);
        setIsModalVisible(true);
    };

    const handleDeleteWorkflow = async (id: number) => {
        Modal.confirm({
            title: 'Xác nhận xóa',
            content: 'Bạn có chắc chắn muốn xóa workflow này không?',
            onOk: async () => {
                try {
                    await api.delete(`/workflows/${id}`);
                    message.success('Xóa workflow thành công!');
                    fetchWorkflows();
                } catch (error) {
                    message.error('Không thể xóa workflow.');
                }
            }
        });
    };

    const columns = [
        {
            title: 'Tên Workflow',
            dataIndex: 'name',
            key: 'name',
            render: (text: string) => (
                <Space>
                    <RocketOutlined className="text-primary" />
                    <span className="font-semibold">{text}</span>
                </Space>
            ),
        },
        {
            title: 'Phiên bản',
            dataIndex: 'version',
            key: 'version',
        },
        {
            title: 'Đường dẫn Package',
            dataIndex: 'packagePath',
            key: 'packagePath',
            render: (path: string) => <code className="bg-slate-100 p-1 rounded text-xs">{path}</code>,
        },
        {
            title: 'Trạng thái',
            dataIndex: 'isActive',
            key: 'isActive',
            render: (active: boolean) => (
                <Tag color={active ? 'blue' : 'default'}>{active ? 'Đang hoạt động' : 'Tạm dừng'}</Tag>
            ),
        },
        {
            title: 'Thao tác',
            key: 'action',
            render: (_: any, record: Workflow) => (
                <Space size="middle">
                    <Button type="link" onClick={() => handleOpenEdit(record)}>Sửa</Button>
                    <Button type="link" danger onClick={() => handleDeleteWorkflow(record.id)}>Xóa</Button>
                </Space>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <Title level={2} className="!m-0">Quản lý Workflows</Title>
                    <Typography.Text className="text-slate-500">Quản lý các quy trình RPA và phiên bản gói cài đặt</Typography.Text>
                </div>
                <Space>
                    <Button icon={<ReloadOutlined />} onClick={fetchWorkflows}>Làm mới</Button>
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        className="shadow-lg shadow-blue-500/30"
                        onClick={handleOpenAdd}
                    >
                        Thêm Workflow
                    </Button>
                </Space>
            </div>

            <Card className="shadow-sm border-0">
                <Table columns={columns} dataSource={workflows} rowKey="id" loading={loading} />
            </Card>

            <Modal
                title={editingWorkflow ? "Chỉnh sửa Workflow" : "Thêm Workflow mới"}
                open={isModalVisible}
                onOk={() => form.submit()}
                onCancel={() => setIsModalVisible(false)}
                confirmLoading={confirmLoading}
                destroyOnClose
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleSaveWorkflow}
                    initialValues={{ version: '1.0.0' }}
                >
                    <Form.Item
                        name="name"
                        label="Tên Workflow"
                        rules={[{ required: true, message: 'Vui lòng nhập tên workflow!' }]}
                    >
                        <Input placeholder="Ví dụ: Invoice Processing" />
                    </Form.Item>
                    <Form.Item
                        name="packagePath"
                        label="Đường dẫn Package"
                        rules={[{ required: true, message: 'Vui lòng nhập đường dẫn package!' }]}
                        help="Đường dẫn vật lý đến file .nupkg hoặc Project.json"
                    >
                        <Input placeholder="C:\RPA\Packages\Workflow.1.0.0.nupkg" />
                    </Form.Item>
                    <Form.Item
                        name="version"
                        label="Phiên bản"
                        rules={[{ required: true, message: 'Vui lòng nhập phiên bản!' }]}
                    >
                        <Input placeholder="1.0.0" />
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
};

export default WorkflowsPage;
