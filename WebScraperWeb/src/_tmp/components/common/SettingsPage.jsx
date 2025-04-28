import React, { useState, useEffect } from 'react';
import { 
  Typography, 
  Tabs, 
  Form, 
  Input, 
  Button, 
  Select, 
  Switch, 
  InputNumber, 
  Card,
  message,
  Space,
  Divider
} from 'antd';
import { 
  SettingOutlined, 
  CloudOutlined, 
  DatabaseOutlined, 
  SafetyCertificateOutlined,
  ApiOutlined,
  SaveOutlined,
  SyncOutlined
} from '@ant-design/icons';
import { getSystemSettings, updateSystemSettings } from '../api/state';

const { Title, Paragraph } = Typography;
const { TabPane } = Tabs;
const { Option } = Select;

/**
 * Global system settings page
 */
const Settings = () => {
  const [loading, setLoading] = useState(false);
  const [settings, setSettings] = useState({
    general: {
      outputDirectory: 'ScrapedData',
      maxConcurrentRequests: 5,
      requestTimeoutSeconds: 30,
      userAgent: 'WebScraper Backoffice 1.0'
    },
    storage: {
      enableCompression: true,
      compressionLevel: 'medium',
      retentionDays: 30,
      storageLimit: 1024,
      backupEnabled: false,
      backupFrequency: 'daily'
    },
    security: {
      requireAuthentication: true,
      sessionTimeout: 60,
      apiKeyEnabled: false,
      rateLimitRequests: 100,
      rateLimitPeriod: 'minute'
    },
    integrations: {
      mongoDbConnection: '',
      redisConnection: '',
      webhookDefaultUrl: '',
      enableExternalServices: false
    }
  });

  // Load settings on component mount
  useEffect(() => {
    fetchSettings();
  }, []);

  const fetchSettings = async () => {
    try {
      setLoading(true);
      // In a real implementation, this would fetch from the API
      const response = await getSystemSettings();
      if (response) {
        setSettings(response);
      }
    } catch (error) {
      console.error('Error fetching settings:', error);
      message.error('Failed to load system settings');
    } finally {
      setLoading(false);
    }
  };

  const handleSaveSettings = async (section, values) => {
    try {
      setLoading(true);
      
      // Update settings state
      const updatedSettings = {
        ...settings,
        [section]: values
      };
      
      // Save to backend
      await updateSystemSettings(updatedSettings);
      
      // Update local state
      setSettings(updatedSettings);
      
      message.success(`${section.charAt(0).toUpperCase() + section.slice(1)} settings saved successfully`);
    } catch (error) {
      console.error('Error saving settings:', error);
      message.error('Failed to save settings');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <Title level={2}>
        <SettingOutlined /> System Settings
      </Title>
      <Paragraph>
        Configure global system settings for the WebScraper Backoffice application.
        These settings affect all scrapers and operations.
      </Paragraph>

      <Tabs defaultActiveKey="general">
        <TabPane 
          tab={<span><SettingOutlined /> General</span>} 
          key="general"
        >
          <Card bordered={false}>
            <Form
              layout="vertical"
              initialValues={settings.general}
              onFinish={(values) => handleSaveSettings('general', values)}
            >
              <Form.Item
                label="Default Output Directory"
                name="outputDirectory"
                rules={[{ required: true, message: 'Please enter an output directory' }]}
              >
                <Input placeholder="ScrapedData" />
              </Form.Item>

              <Form.Item
                label="Maximum Concurrent Requests"
                name="maxConcurrentRequests"
                rules={[{ required: true, message: 'Please enter maximum concurrent requests' }]}
              >
                <InputNumber min={1} max={20} />
              </Form.Item>

              <Form.Item
                label="Request Timeout (Seconds)"
                name="requestTimeoutSeconds"
                rules={[{ required: true, message: 'Please enter request timeout' }]}
              >
                <InputNumber min={5} max={120} />
              </Form.Item>

              <Form.Item
                label="Default User Agent"
                name="userAgent"
                rules={[{ required: true, message: 'Please enter a user agent' }]}
              >
                <Input placeholder="WebScraper Backoffice 1.0" />
              </Form.Item>

              <Form.Item>
                <Button 
                  type="primary" 
                  htmlType="submit" 
                  loading={loading}
                  icon={<SaveOutlined />}
                >
                  Save Settings
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </TabPane>

        <TabPane 
          tab={<span><CloudOutlined /> Storage</span>} 
          key="storage"
        >
          <Card bordered={false}>
            <Form
              layout="vertical"
              initialValues={settings.storage}
              onFinish={(values) => handleSaveSettings('storage', values)}
            >
              <Form.Item
                label="Enable Content Compression"
                name="enableCompression"
                valuePropName="checked"
              >
                <Switch />
              </Form.Item>

              <Form.Item
                label="Compression Level"
                name="compressionLevel"
                rules={[{ required: true, message: 'Please select compression level' }]}
              >
                <Select>
                  <Option value="low">Low</Option>
                  <Option value="medium">Medium</Option>
                  <Option value="high">High</Option>
                </Select>
              </Form.Item>

              <Form.Item
                label="Data Retention (Days)"
                name="retentionDays"
                rules={[{ required: true, message: 'Please enter retention days' }]}
              >
                <InputNumber min={1} max={365} />
              </Form.Item>

              <Form.Item
                label="Storage Limit (MB)"
                name="storageLimit"
                rules={[{ required: true, message: 'Please enter storage limit' }]}
              >
                <InputNumber min={100} step={100} />
              </Form.Item>

              <Form.Item
                label="Enable Automatic Backups"
                name="backupEnabled"
                valuePropName="checked"
              >
                <Switch />
              </Form.Item>

              <Form.Item
                label="Backup Frequency"
                name="backupFrequency"
                rules={[{ required: true, message: 'Please select backup frequency' }]}
              >
                <Select>
                  <Option value="hourly">Hourly</Option>
                  <Option value="daily">Daily</Option>
                  <Option value="weekly">Weekly</Option>
                  <Option value="monthly">Monthly</Option>
                </Select>
              </Form.Item>

              <Form.Item>
                <Button 
                  type="primary" 
                  htmlType="submit" 
                  loading={loading}
                  icon={<SaveOutlined />}
                >
                  Save Settings
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </TabPane>

        <TabPane 
          tab={<span><SafetyCertificateOutlined /> Security</span>} 
          key="security"
        >
          <Card bordered={false}>
            <Form
              layout="vertical"
              initialValues={settings.security}
              onFinish={(values) => handleSaveSettings('security', values)}
            >
              <Form.Item
                label="Require Authentication"
                name="requireAuthentication"
                valuePropName="checked"
              >
                <Switch />
              </Form.Item>

              <Form.Item
                label="Session Timeout (Minutes)"
                name="sessionTimeout"
                rules={[{ required: true, message: 'Please enter session timeout' }]}
              >
                <InputNumber min={5} max={1440} />
              </Form.Item>

              <Form.Item
                label="Enable API Key Authentication"
                name="apiKeyEnabled"
                valuePropName="checked"
              >
                <Switch />
              </Form.Item>

              <Divider>Rate Limiting</Divider>

              <Form.Item
                label="Request Limit"
                name="rateLimitRequests"
                rules={[{ required: true, message: 'Please enter rate limit requests' }]}
              >
                <InputNumber min={1} />
              </Form.Item>

              <Form.Item
                label="Rate Limit Period"
                name="rateLimitPeriod"
                rules={[{ required: true, message: 'Please select rate limit period' }]}
              >
                <Select>
                  <Option value="second">Per Second</Option>
                  <Option value="minute">Per Minute</Option>
                  <Option value="hour">Per Hour</Option>
                  <Option value="day">Per Day</Option>
                </Select>
              </Form.Item>

              <Form.Item>
                <Space>
                  <Button 
                    type="primary" 
                    htmlType="submit" 
                    loading={loading}
                    icon={<SaveOutlined />}
                  >
                    Save Settings
                  </Button>
                  <Button
                    type="default"
                    onClick={() => message.info('This would generate a new API key in a real implementation')}
                    icon={<SyncOutlined />}
                    disabled={!settings.security.apiKeyEnabled}
                  >
                    Regenerate API Key
                  </Button>
                </Space>
              </Form.Item>
            </Form>
          </Card>
        </TabPane>

        <TabPane 
          tab={<span><DatabaseOutlined /> Integrations</span>} 
          key="integrations"
        >
          <Card bordered={false}>
            <Form
              layout="vertical"
              initialValues={settings.integrations}
              onFinish={(values) => handleSaveSettings('integrations', values)}
            >
              <Form.Item
                label="MongoDB Connection String"
                name="mongoDbConnection"
                rules={[{ required: false, message: 'Please enter MongoDB connection string' }]}
                help="Leave empty to use application defaults"
              >
                <Input.Password placeholder="mongodb://username:password@host:port/database" />
              </Form.Item>

              <Form.Item
                label="Redis Connection String"
                name="redisConnection"
                rules={[{ required: false, message: 'Please enter Redis connection string' }]}
                help="Leave empty to use application defaults"
              >
                <Input.Password placeholder="redis://username:password@host:port" />
              </Form.Item>

              <Form.Item
                label="Default Webhook URL"
                name="webhookDefaultUrl"
                rules={[{ required: false, type: 'url', message: 'Please enter a valid URL' }]}
                help="Default URL for webhook notifications"
              >
                <Input placeholder="https://example.com/webhook" />
              </Form.Item>

              <Form.Item
                label="Enable External Services"
                name="enableExternalServices"
                valuePropName="checked"
              >
                <Switch />
              </Form.Item>

              <Form.Item>
                <Button 
                  type="primary" 
                  htmlType="submit" 
                  loading={loading}
                  icon={<SaveOutlined />}
                >
                  Save Settings
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </TabPane>

        <TabPane 
          tab={<span><ApiOutlined /> API</span>} 
          key="api"
        >
          <Card bordered={false}>
            <Title level={4}>API Documentation</Title>
            <Paragraph>
              The WebScraper Backoffice provides a comprehensive API for programmatic control
              of all scraper operations. Use this API to integrate with your own systems or
              automate workflows.
            </Paragraph>
            
            <Divider />
            
            <Title level={5}>Base URL</Title>
            <Input readOnly value="https://your-instance.example.com/api" />
            
            <Divider />
            
            <Title level={5}>Authentication</Title>
            <Paragraph>
              API requests are authenticated using an API key that should be included in the
              "x-api-key" header of all requests.
            </Paragraph>
            
            <Divider />
            
            <Title level={5}>Available Endpoints</Title>
            <ul>
              <li><strong>/api/scrapers</strong> - Manage scraper configurations</li>
              <li><strong>/api/monitoring</strong> - Access monitoring data</li>
              <li><strong>/api/analytics</strong> - Retrieve analytics information</li>
              <li><strong>/api/scheduling</strong> - Control scraper scheduling</li>
              <li><strong>/api/notifications</strong> - Configure webhook notifications</li>
            </ul>
            
            <Button 
              type="primary" 
              href="/api-docs" 
              target="_blank"
              icon={<ApiOutlined />}
            >
              View Full API Documentation
            </Button>
          </Card>
        </TabPane>
      </Tabs>
    </div>
  );
};

export default Settings;