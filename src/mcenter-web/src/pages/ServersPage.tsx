import React, { useEffect, useState } from 'react';
import { Table, Card, Button, Typography, Space, message, Badge, Modal, Form, Input } from 'antd';
import { ReloadOutlined, PlusOutlined, DesktopOutlined } from '@ant-design/icons';
import api from '../api/axios';
import type { Server } from '../types';
import dayjs from 'dayjs';

const { Title } = Typography;

const ServersPage: React.FC = () => {
    const [servers, setServers] = useState<Server[]>([]);
    const [loading, setLoading] = useState(false);
    const [isModalVisible, setIsModalVisible] = useState(false);
    const [editingServer, setEditingServer] = useState<Server | null>(null);
    const [confirmLoading, setConfirmLoading] = useState(false);
    const [form] = Form.useForm();

    const fetchServers = async () => {
        setLoading(true);
        try {
            const response = await api.get('/servers');
            setServers(response.data);
        } catch (error) {
            message.error('Không thể tải danh sách máy chủ.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchServers();
        // Refresh every 30 seconds
        const interval = setInterval(fetchServers, 30000);
        return () => clearInterval(interval);
    }, []);

    const handleSaveServer = async (values: any) => {
        setConfirmLoading(true);
        try {
            if (editingServer) {
                await api.put(`/servers/${editingServer.id}`, values);
                message.success('Cập nhật máy chủ thành công!');
            } else {
                await api.post('/servers', values);
                message.success('Thêm máy chủ thành công!');
            }
            setIsModalVisible(false);
            setEditingServer(null);
            form.resetFields();
            fetchServers();
        } catch (error: any) {
            message.error(error.response?.data?.message || 'Thao tác thất bại.');
        } finally {
            setConfirmLoading(false);
        }
    };

    const handleOpenAdd = () => {
        setEditingServer(null);
        form.resetFields();
        setIsModalVisible(true);
    };

    const handleOpenEdit = (server: Server) => {
        setEditingServer(server);
        form.setFieldsValue(server);
        setIsModalVisible(true);
    };

    const handleDeleteServer = async (id: number) => {
        Modal.confirm({
            title: 'Xác nhận xóa',
            content: 'Bạn có chắc chắn muốn xóa máy chủ này không?',
            onOk: async () => {
                try {
                    await api.delete(`/servers/${id}`);
                    message.success('Xóa máy chủ thành công!');
                    fetchServers();
                } catch (error) {
                    message.error('Không thể xóa máy chủ.');
                }
            }
        });
    };

    const columns = [
        {
            title: 'Tên máy chủ',
            dataIndex: 'name',
            key: 'name',
            render: (text: string) => (
                <Space>
                    <DesktopOutlined className="text-primary" />
                    <span className="font-semibold">{text}</span>
                </Space>
            ),
        },
        {
            title: 'Địa chỉ IP',
            dataIndex: 'ipAddress',
            key: 'ipAddress',
        },
        {
            title: 'Trạng thái',
            dataIndex: 'status',
            key: 'status',
            render: (status: string) => {
                let color = 'default';
                if (status === 'Online') color = 'success';
                if (status === 'Offline') color = 'error';
                if (status === 'Busy') color = 'warning';
                return <Badge status={color as any} text={status} />;
            },
        },
        {
            title: 'Heartbeat cuối',
            dataIndex: 'lastHeartbeat',
            key: 'lastHeartbeat',
            render: (time: string) => time ? dayjs(time).format('DD/MM/YYYY HH:mm:ss') : 'Chưa có dữ liệu',
        },
        {
            title: 'Thao tác',
            key: 'action',
            render: (_: any, record: Server) => (
                <Space size="middle">
                    <Button type="link" onClick={() => handleOpenEdit(record)}>Sửa</Button>
                    <Button type="link" danger onClick={() => handleDeleteServer(record.id)}>Xóa</Button>
                </Space>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <Title level={2} className="!m-0">Quản lý máy chủ</Title>
                    <Typography.Text className="text-slate-500">Theo dõi trạng thái và thông tin các Agent RPA</Typography.Text>
                </div>
                <Space>
                    <Button icon={<ReloadOutlined />} onClick={fetchServers} loading={loading}>Làm mới</Button>
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        className="shadow-lg shadow-blue-500/30"
                        onClick={handleOpenAdd}
                    >
                        Thêm máy chủ
                    </Button>
                </Space>
            </div>

            <Card className="shadow-sm border-0">
                <Table
                    columns={columns}
                    dataSource={servers}
                    rowKey="id"
                    loading={loading}
                    pagination={{ pageSize: 10 }}
                />
            </Card>

            <Modal
                title={editingServer ? "Chỉnh sửa máy chủ" : "Thêm máy chủ mới"}
                open={isModalVisible}
                onOk={() => form.submit()}
                onCancel={() => setIsModalVisible(false)}
                confirmLoading={confirmLoading}
                destroyOnClose
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleSaveServer}
                    initialValues={{ status: 'Online' }}
                >
                    <Form.Item
                        name="name"
                        label="Tên máy chủ"
                        rules={[{ required: true, message: 'Vui lòng nhập tên máy chủ!' }]}
                    >
                        <Input placeholder="Ví dụ: RPA-WKS-01" />
                    </Form.Item>
                    <Form.Item
                        name="ipAddress"
                        label="Địa chỉ IP"
                        rules={[{ required: true, message: 'Vui lòng nhập địa chỉ IP!' }]}
                    >
                        <Input placeholder="Ví dụ: 192.168.1.10" />
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
};

export default ServersPage;
