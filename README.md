# Wpf常用库

> 使用.net48 构建的wpf常用工具库，封装了mvvm、依赖注入、消息通知、常用工具等。
>
> 作者: love-yuri

## 快速开始

1. 在项目中引入依赖: `<PackageReference Include="love-yuri.WpfCommon" />`

2. 在App.xaml中添加项目核心样式

   ```xaml
   <ResourceDictionary Source="pack://application:,,,/love-yuri;component/Styles/Mian.xaml" />
   
   <!-- 完整示例 -->
   <Application.Resources>
       <ResourceDictionary>
           <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/love-yuri;component/Styles/Mian.xaml" />
            </ResourceDictionary.MergedDictionaries>
       </ResourceDictionary>
   </Application.Resources>
   ```

3. 直接开始使用

## 项目依赖

- `wpf`

## Api

### Mvvm
> 位于项目: `Core/Mvvm` 目录下
>
> 封装了mvvm常用的工具架构

1. 